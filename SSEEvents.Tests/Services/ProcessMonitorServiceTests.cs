using NUnit.Framework;
using SSEEvents.Client.Services;
using System;

namespace SSEEvents.Tests
{
    public class Tests
    {
        private ProcessMonitorService _processMonitorService;

        [OneTimeSetUp]
        public void Setup()
        {
            _processMonitorService = new ProcessMonitorService(null);
        }

        [Test]
        [TestCase(2, 10, 80)]
        [TestCase(2, 16, 87.5)]
        [TestCase(16, 32, 50)]
        public void CalculateMemoryUtilizationPercentage_MemoryUtilization_ResponseIsCorrect(double availableMemory, double totalMemory, double expectedResult)
        {
            var actualResult = _processMonitorService.CalculateMemoryUtilizationPercentage(availableMemory, totalMemory);

            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test]
        [TestCase(1610612736.0, 1.5)]
        [TestCase(536870912.0, 0.5)]
        [TestCase(268435456.0, 0.25)]
        public void ConvertBytesToGB_Conversion_ResponseIsCorrect(double bytes, double expectedResult)
        {
            double tolerance = 1.0e-10;

            var actualResult = _processMonitorService.ConvertBytesToGB(bytes);

            Assert.That(actualResult, Is.EqualTo(expectedResult).Within(tolerance));
        }

        [Test]
        [TestCase(1024, 1)]
        [TestCase(512, 0.50)]
        [TestCase(4096, 4)]
        public void ConvertMBToGB_Conversion_ResponseIsCorrect(double megabytes, double expectedResult)
        {
            double tolerance = 1.0e-10;

            var actualResult = _processMonitorService.ConvertMBToGB(megabytes);

            Assert.That(actualResult, Is.EqualTo(expectedResult).Within(tolerance));
        }

        [Test]
        [TestCase((UInt64)1048576, 1)]
        [TestCase((UInt64)2097152, 2)]
        public void ConvertKBToGB_Conversion_ResponseIsCorrect(UInt64 kilobytes, double expectedResult)
        {
            double tolerance = 1.0e-10;

            var actualResult = _processMonitorService.ConvertKBToGB(kilobytes);

            Assert.That(actualResult, Is.EqualTo(expectedResult).Within(tolerance));
        }

        [Test]
        [TestCase(1048576, 1)]
        [TestCase(2097152, 2)]
        public void ConvertBytesToMB_Conversion_ResponseIsCorrect(long bytes, double expectedResult)
        {
            double tolerance = 1.0e-10;

            var actualResult = _processMonitorService.ConvertBytesToMB(bytes);

            Assert.That(actualResult, Is.EqualTo(expectedResult).Within(tolerance));
        }
    }
}