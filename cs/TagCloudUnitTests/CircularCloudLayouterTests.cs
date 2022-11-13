using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using TagCloud;

namespace TagCloudUnitTests
{
    [TestFixture]
    internal class CircularCloudLayouterTests
    {
        private CircularCloudLayouter layouter;

        private IReadOnlyList<Rectangle> layout;

        [SetUp]
        public void Setup()
        {
            layouter = new CircularCloudLayouter(new Point(0, 0));

            layout = new List<Rectangle>();
        }

        [TearDown]
        public void TearDown()
        {
            if (layout.Count > 0 && IsTestResultStateFailureOrError())
            {
                GenerateAndSaveCloudImage();
            }
        }

        [Test]
        public void PutNextRectangle_PutsRectangleInTheCenter_WhenFirstRectangleAdded()
        {
            layout = GetLayout(new[] { new Size(100, 50) });

            layout.First().GetCenter().Should().BeEquivalentTo(layouter.CloudCenter);
        }

        [TestCase(-1, 1, TestName = "negative width")]
        [TestCase(1, -1, TestName = "negative height")]
        [TestCase(-1, -1, TestName = "negative width and height")]
        [TestCase(0, 1, TestName = "zero width")]
        [TestCase(1, 0, TestName = "zero height")]
        [TestCase(0, 0, TestName = "zero width and height")]
        public void PutNextRectangle_ThrowsArgumentException_WhenSizeIsInvalid(int width, int height)
        {
            Action action = () => layouter.PutNextRectangle(new Size(width, height));

            action.Should().Throw<ArgumentException>().WithMessage("width and height of rectangle must be more than zero");
        }

        [Test]
        public void PutNextRectangle_PutsTwoAlignedHorizontallyRectangles_WhenTwoAdded()
        {
            var sizes = new[]
            {
                new Size(100, 150),
                new Size(100, 150)
            };

            layout = GetLayout(sizes);

            var firstRectangle = layout[0];
            var secondRectangle = layout[1];

            Math.Abs(firstRectangle.Y).Should().Be(Math.Abs(secondRectangle.Y));
        }

        [Test]
        public void PutNextRectangle_PutsTwoAlignedVerticallyRectangles_WhenTwoAdded()
        {
            var sizes = new[]
            {
                new Size(150, 100),
                new Size(150, 100)
            };

            layout = GetLayout(sizes);

            var firstRectangle = layout[0];
            var secondRectangle = layout[1];

            Math.Abs(firstRectangle.X).Should().Be(Math.Abs(secondRectangle.X));
        }
        
        [TestCase(2)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        public void PutNextRectangle_PutsNonIntersectingRectangles_WhenSeveralRectanglesAdded(int rectanglesCount)
        {
            var sizes = RectangleSizeGenerator.GetConstantSizes(rectanglesCount, new Size(100, 50));

            layout = GetLayout(sizes);

            IsAnyIntersection(layout).Should().Be(false);
        }

        private bool IsAnyIntersection(IReadOnlyList<Rectangle> cloudLayout)
        {
            foreach (var rectangle in cloudLayout)
                if (cloudLayout.Where(r => !r.Equals(rectangle)).Any(r => r.IntersectsWith(rectangle)))
                    return true;

            return false;
        }

        private IReadOnlyList<Rectangle> GetLayout(IEnumerable<Size> rectanglesSizes)
        {
            var rectangles = rectanglesSizes.Select(size => layouter.PutNextRectangle(size)).ToList();

            return rectangles.AsReadOnly();
        }

        public bool IsTestResultStateFailureOrError()
        {
            var resultState = TestContext.CurrentContext.Result.Outcome;

            return resultState == ResultState.Failure || resultState == ResultState.Error;
        }

        private void GenerateAndSaveCloudImage()
        {
            var imageCreator = new CloudImageGenerator(layouter, Color.Black);

            var image = imageCreator.GenerateBitmap(layout);

            ErrorTestImageSaver.SaveBitmap(image, out var fullPath);

            TestContext.Out.WriteLine("Tag cloud visualization saved to file " + fullPath);
        }


    }
}