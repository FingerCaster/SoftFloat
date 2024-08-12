using SoftFloat;
using System;
using System.Diagnostics;

namespace SoftNumberTest
{
    public static class Tests_sdouble
    {
        const int RandomTestCount = 100_000;

        public static void RunAllTests()
        {
            TestAddition();
            TestSubtraction();
            TestMultiplication();
            TestDivision();

            RandomTestUnaryOperations();
            RandomTestBinaryOperations();
        }

        private static void RandomTestUnaryOperations()
        {
            RandomTestUnaryOperation(UnaryOperationType.Round);
            RandomTestUnaryOperation(UnaryOperationType.Floor);
            RandomTestUnaryOperation(UnaryOperationType.Ceiling);

            RandomTestTrigonometryOperation(UnaryOperationType.Sine);
            RandomTestTrigonometryOperation(UnaryOperationType.Cosine);

            RandomTestUnaryOperation(UnaryOperationType.SquareRoot);
            RandomTestUnaryOperation(UnaryOperationType.Exponential, 100.0);

            RandomTestUnaryOperation(UnaryOperationType.LogarithmNatural);
            RandomTestUnaryOperation(UnaryOperationType.LogarithmBase2);

            RandomTestUnaryOperation(UnaryOperationType.ArcSine);
            RandomTestUnaryOperation(UnaryOperationType.ArcCosine);
            RandomTestUnaryOperation(UnaryOperationType.ArcTangent);
        }

        private static void RandomTestBinaryOperations()
        {
            RandomTestBinaryOperation(BinaryOperationType.Addition);
            RandomTestBinaryOperation(BinaryOperationType.Subtraction);
            RandomTestBinaryOperation(BinaryOperationType.Multiplication);
            RandomTestBinaryOperation(BinaryOperationType.Division);
            RandomTestBinaryOperation(BinaryOperationType.Modulus);

            RandomTestBinaryOperation(BinaryOperationType.Power);
            RandomTestBinaryOperation(BinaryOperationType.ArcTangent2);
        }

        private delegate sdouble BinaryOperation(sdouble a, sdouble b);

        private enum BinaryOperationType : int
        {
            Addition, Subtraction, Multiplication, Division, Modulus, Power, ArcTangent2
        }

        private static readonly BinaryOperation[] binaryOperations = new BinaryOperation[]
        {
            (a, b) => a + b,
            (a, b) => a - b,
            (a, b) => a * b,
            (a, b) => a / b,
            (a, b) => a % b,
            libm_sdouble.pow,
            libm_sdouble.atan2
        };

        private static void TestBinaryOperationDoubleExact(double a, double b, double expected, BinaryOperationType op)
        {
            BinaryOperation func = binaryOperations[(int)op];
            sdouble result = func((sdouble)a, (sdouble)b);
            bool isOk = result.Equals((sdouble)expected);
            Debug.Assert(isOk);
        }

        private static void TestBinaryOperationDoubleApproximate(double a, double b, double expected, BinaryOperationType op)
        {
            BinaryOperation func = binaryOperations[(int)op];
            sdouble result = func((sdouble)a, (sdouble)b);

            if (double.IsNaN(expected) && result.IsNaN())
            {
                return;
            }

            if (double.IsInfinity(expected) && result.IsInfinity() && Math.Sign(expected) == result.Sign())
            {
                return;
            }

            double allowedError = Math.Max(1e-12 * Math.Pow(2.0d, Math.Log2(Math.Abs(expected) + 1.0d)), 1e-12);
            double difference = Math.Abs((double)result - expected);
            bool isOk = difference < allowedError;
            Debug.Assert(isOk);
        }

        private enum UnaryOperationType : int
        {
            Round, Floor, Ceiling, Sine, Cosine, Tangent, SquareRoot, Exponential, LogarithmNatural, LogarithmBase2,
            ArcSine, ArcCosine, ArcTangent
        }

        private delegate sdouble UnaryOperation(sdouble x);

        private static readonly UnaryOperation[] unaryOperations = new UnaryOperation[]
        {
            libm_sdouble.round,
            libm_sdouble.floor,
            libm_sdouble.ceil,
            libm_sdouble.sin,
            libm_sdouble.cos,
            libm_sdouble.tan,
            libm_sdouble.sqrt,
            libm_sdouble.exp,
            libm_sdouble.log,
            libm_sdouble.log2,
            libm_sdouble.asin,
            libm_sdouble.acos,
            libm_sdouble.atan
        };

        private static void TestUnaryOperationDoubleExact(double x, double expected, UnaryOperationType op)
        {
            UnaryOperation func = unaryOperations[(int)op];
            sdouble result = func((sdouble)x);
            bool isOk = result.Equals((sdouble)expected);
            Debug.Assert(isOk);
        }

        private static void TestUnaryOperationDoubleApproximate(double x, double expected, UnaryOperationType op, double allowedErrorMultiplier = 1.0)
        {
            UnaryOperation func = unaryOperations[(int)op];
            sdouble result = func((sdouble)x);

            if (double.IsNaN(expected) && result.IsNaN())
            {
                return;
            }

            if (double.IsInfinity(expected) && result.IsInfinity() && Math.Sign(expected) == result.Sign())
            {
                return;
            }

            double allowedError = Math.Max(1e-12 * allowedErrorMultiplier * Math.Pow(2.0, Math.Log2(Math.Abs(expected) + 1.0)), 1e-12);
            double difference = Math.Abs((double)result - expected);
            bool isOk = difference <= allowedError;

            Debug.Assert(isOk);
        }

        private static void TestTrigonometryOperationApproximate(double x, double expected, UnaryOperationType op)
        {
            UnaryOperation func = unaryOperations[(int)op];
            sdouble result = func((sdouble)x);

            if (double.IsNaN(expected) && result.IsNaN())
            {
                return;
            }

            if (double.IsInfinity(expected) && result.IsInfinity() && Math.Sign(expected) == result.Sign())
            {
                return;
            }

            double allowedError = Math.Max(0.005 * Math.Pow(2.0, Math.Log2(Math.Abs(expected) + 1.0)), 1e-12);
            double difference = Math.Abs((double)result - expected);
            bool isOk = difference <= allowedError;

            Debug.Assert(isOk);
        }

        private static void RandomTestBinaryOperation(BinaryOperationType op)
        {
            Func<double, double, double> func = op switch
            {
                BinaryOperationType.Addition => (double a, double b) => a + b,
                BinaryOperationType.Subtraction => (double a, double b) => a - b,
                BinaryOperationType.Multiplication => (double a, double b) => a * b,
                BinaryOperationType.Division => (double a, double b) => a / b,
                BinaryOperationType.Modulus => (double a, double b) => a % b,
                BinaryOperationType.Power => Math.Pow,
                BinaryOperationType.ArcTangent2 => Math.Atan2,
                _ => throw new ArgumentException(),
            };

            PCG rand = new PCG(0, 0);

            for (int i = 0; i < RandomTestCount; ++i)
            {
                double a = rand.DoubleInclusive(-1e-10, 1e-10);
                double b = rand.DoubleInclusive(-1e-10, 1e-10);
                TestBinaryOperationDoubleApproximate(a, b, func(a, b), op);
            }

            for (int i = 0; i < RandomTestCount; ++i)
            {
                double a = rand.DoubleInclusive(-1.0, 1.0);
                double b = rand.DoubleInclusive(-1.0, 1.0);
                TestBinaryOperationDoubleApproximate(a, b, func(a, b), op);
            }

            for (int i = 0; i < RandomTestCount; ++i)
            {
                double a = rand.DoubleInclusive(-100000.0, 100000.0);
                double b = rand.DoubleInclusive(-100000.0, 100000.0);
                TestBinaryOperationDoubleApproximate(a, b, func(a, b), op);
            }

            for (int i = 0; i < RandomTestCount; ++i)
            {
                double a = rand.DoubleInclusive(-1000000000.0, 1000000000.0);
                double b = rand.DoubleInclusive(-1000000000.0, 1000000000.0);
                TestBinaryOperationDoubleApproximate(a, b, func(a, b), op);
            }

            for (int i = 0; i < RandomTestCount; ++i)
            {
                double a = rand.DoubleInclusive(-1e38, 1e38);
                double b = rand.DoubleInclusive(-1e38, 1e38);
                TestBinaryOperationDoubleApproximate(a, b, func(a, b), op);
            }
        }

        private static void RandomTestUnaryOperation(UnaryOperationType op, double allowedErrorMultiplier = 1.0)
        {
            Func<double, double> func = op switch
            {
                UnaryOperationType.Round => Math.Round,
                UnaryOperationType.Floor => Math.Floor,
                UnaryOperationType.Ceiling => Math.Ceiling,
                UnaryOperationType.Sine => Math                .Sin,
                UnaryOperationType.Cosine => Math.Cos,
                UnaryOperationType.Tangent => Math.Tan,
                UnaryOperationType.SquareRoot => Math.Sqrt,
                UnaryOperationType.Exponential => Math.Exp,
                UnaryOperationType.LogarithmNatural => Math.Log,
                UnaryOperationType.LogarithmBase2 => Math.Log2,
                UnaryOperationType.ArcSine => Math.Asin,
                UnaryOperationType.ArcCosine => Math.Acos,
                UnaryOperationType.ArcTangent => Math.Atan,
                _ => throw new ArgumentException(),
            };

            PCG rand = new PCG(0, 0);

            // very small values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                double x = rand.DoubleInclusive(-1e-40, 1e-40);
                TestUnaryOperationDoubleApproximate(x, func(x), op, allowedErrorMultiplier);
            }

            // small values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                double x = rand.DoubleInclusive(-1.0, 1.0);
                TestUnaryOperationDoubleApproximate(x, func(x), op, allowedErrorMultiplier);
            }

            // large values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                double x = rand.DoubleInclusive(-100000.0, 100000.0);
                TestUnaryOperationDoubleApproximate(x, func(x), op, allowedErrorMultiplier);
            }

            // huge values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                double x = rand.DoubleInclusive(-1000000000.0, 1000000000.0);
                TestUnaryOperationDoubleApproximate(x, func(x), op, allowedErrorMultiplier);
            }
        }

        private static void RandomTestTrigonometryOperation(UnaryOperationType op)
        {
            Func<double, double> func = op switch
            {
                UnaryOperationType.Sine => Math.Sin,
                UnaryOperationType.Cosine => Math.Cos,
                UnaryOperationType.Tangent => Math.Tan,
                _ => throw new ArgumentException(),
            };

            PCG rand = new PCG(0, 0);

            // small values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                double x = rand.DoubleInclusive(-1.0, 1.0);
                TestTrigonometryOperationApproximate(x, func(x), op);
            }

            // medium values
            for (int i = 0; i < RandomTestCount; ++i)
            {
                double x = rand.DoubleInclusive(-100.0, 100.0);
                TestTrigonometryOperationApproximate(x, func(x), op);
            }
        }

        public static void TestAddition()
        {
            const BinaryOperationType op = BinaryOperationType.Addition;

            TestBinaryOperationDoubleExact(0.0d, 0.0d, 0.0d, op);
            TestBinaryOperationDoubleExact(1.0d, 0.0d, 1.0d, op);
            TestBinaryOperationDoubleExact(0.0d, 1.0d, 1.0d, op);

            TestBinaryOperationDoubleExact(-0.0d, 0.0d, 0.0d, op);
            TestBinaryOperationDoubleExact(-0.0d, 0.0, -0.0d, op);
            TestBinaryOperationDoubleExact(0.0d, 0.0d, -0.0d, op);

            TestBinaryOperationDoubleExact(1.0d, -1.0d, 0.0d, op);
            TestBinaryOperationDoubleExact(-1.0d, -1.0d, -2.0d, op);

            TestBinaryOperationDoubleApproximate(123.456d, 456.789d, 580.245d, op);
            TestBinaryOperationDoubleApproximate(3.4630664266983525E-11, 5.5345556458565979E-11, 8.9976220725549504E-11, op);

            TestBinaryOperationDoubleExact(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, op);
            TestBinaryOperationDoubleExact(double.PositiveInfinity, double.NegativeInfinity, double.NaN, op);

            TestBinaryOperationDoubleExact(double.NaN, double.NaN, double.NaN, op);
            TestBinaryOperationDoubleExact(0.0, double.NaN, double.NaN, op);
            TestBinaryOperationDoubleExact(-999999.0, double.NaN, double.NaN, op);

            RandomTestBinaryOperation(op);
        }

        public static void TestSubtraction()
        {
            const BinaryOperationType op = BinaryOperationType.Subtraction;

            TestBinaryOperationDoubleExact(0.0, 0.0, 0.0, op);
            TestBinaryOperationDoubleExact(1.0, 0.0, 1.0, op);
            TestBinaryOperationDoubleExact(0.0, 1.0, -1.0, op);

            TestBinaryOperationDoubleExact(-0.0, 0.0, 0.0, op);
            TestBinaryOperationDoubleExact(-0.0, 0.0, -0.0, op);
            TestBinaryOperationDoubleExact(0.0, 0.0, -0.0, op);

            TestBinaryOperationDoubleExact(1.0, -1.0, 2.0, op);
            TestBinaryOperationDoubleExact(-1.0, -1.0, 0.0, op);

            TestBinaryOperationDoubleApproximate(123.456, 456.789, -333.333, op);

            TestBinaryOperationDoubleExact(double.PositiveInfinity, double.PositiveInfinity, double.NaN, op);
            TestBinaryOperationDoubleExact(double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity, op);

            TestBinaryOperationDoubleExact(double.NaN, double.NaN, double.NaN, op);
            TestBinaryOperationDoubleExact(0.0, double.NaN, double.NaN, op);
            TestBinaryOperationDoubleExact(-999999.0, double.NaN, double.NaN, op);

            RandomTestBinaryOperation(op);
        }

        public static void TestMultiplication()
        {
            const BinaryOperationType op = BinaryOperationType.Multiplication;

            TestBinaryOperationDoubleExact(0.0, 0.0, 0.0, op);
            TestBinaryOperationDoubleExact(1.0, 0.0, 0.0, op);
            TestBinaryOperationDoubleExact(0.0, 1.0, 0.0, op);

            TestBinaryOperationDoubleExact(-0.0, 0.0, 0.0, op);
            TestBinaryOperationDoubleExact(-0.0, 0.0, -0.0, op);
            TestBinaryOperationDoubleExact(0.0, 0.0, -0.0, op);

            TestBinaryOperationDoubleExact(1.0, -1.0, -1.0, op);
            TestBinaryOperationDoubleExact(-1.0, -1.0, 1.0, op);

            TestBinaryOperationDoubleApproximate(123.456, 456.789, 56393.34, op);
            TestBinaryOperationDoubleApproximate(1e-40, 1e-42, 0.0, op);

            TestBinaryOperationDoubleExact(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, op);
            TestBinaryOperationDoubleExact(double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity, op);
            TestBinaryOperationDoubleExact(double.NegativeInfinity, double.NegativeInfinity, double.PositiveInfinity, op);
            TestBinaryOperationDoubleExact(double.NaN, double.PositiveInfinity, double.NaN, op);
            TestBinaryOperationDoubleExact(0.0, double.PositiveInfinity, double.NaN, op);

            TestBinaryOperationDoubleExact(double.NaN, double.NaN, double.NaN, op);
            TestBinaryOperationDoubleExact(0.0, double.NaN, double.NaN, op);
            TestBinaryOperationDoubleExact(-999999.0, double.NaN, double.NaN, op);

            RandomTestBinaryOperation(op);
        }

        public static void TestDivision()
        {
            const BinaryOperationType op = BinaryOperationType.Division;

            TestBinaryOperationDoubleExact(0.0, 0.0, double.NaN, op);
            TestBinaryOperationDoubleExact(1.0, 0.0, double.PositiveInfinity, op);
            TestBinaryOperationDoubleExact(0.0, 1.0, 0.0, op);

            TestBinaryOperationDoubleExact(-0.0, 0.0, double.NaN, op);
            TestBinaryOperationDoubleExact(-0.0, 0.0, double.NaN, op);
            TestBinaryOperationDoubleExact(0.0, 0.0, double.NaN, op);

            TestBinaryOperationDoubleExact(1.0, -1.0, -1.0, op);
            TestBinaryOperationDoubleExact(-1.0, -1.0, 1.0, op);

            TestBinaryOperationDoubleApproximate(123.456, 456.789, 0.2702692, op);
            TestBinaryOperationDoubleApproximate(1e-40, 1e-42, 99.94678, op);

            TestBinaryOperationDoubleExact(double.PositiveInfinity, double.PositiveInfinity, double.NaN, op);
            TestBinaryOperationDoubleExact(double.PositiveInfinity, double.NegativeInfinity, double.NaN, op);
            TestBinaryOperationDoubleExact(double.NegativeInfinity, double.NegativeInfinity, double.NaN, op);
            TestBinaryOperationDoubleExact(double.NaN, double.PositiveInfinity, double.NaN, op);
            TestBinaryOperationDoubleExact(0.0, double.PositiveInfinity, 0.0, op);
            TestBinaryOperationDoubleExact(double.PositiveInfinity, 0.0, double.PositiveInfinity, op);

            TestBinaryOperationDoubleExact(double.NaN, double.NaN, double.NaN, op);
            TestBinaryOperationDoubleExact(0.0, double.NaN, double.NaN, op);
            TestBinaryOperationDoubleExact(-999999.0, double.NaN, double.NaN, op);

            RandomTestBinaryOperation(op);
        }
    }
}