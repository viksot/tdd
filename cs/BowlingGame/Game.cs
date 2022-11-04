using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using BowlingGame.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace BowlingGame
{
    public class Game
    {
        private List<int> _score = new List<int>();

        private int _frameIndex = 0;

        private bool _firstTryUsed = false;

        public void Roll(int pins)
        {
            CheckCorrectRoll(pins);



            _score.Add(pins);
        }

        private void CheckCorrectRoll(int pins)
        {
            if (pins > 10)
                throw new ArgumentException("Pins count can't be more than ten");

            if (pins < 0)
                throw new ArgumentException("Pins count can't be negative");

            if (_score.Count > 0 && pins > 10 - _score.Last())
                throw new ArgumentException("Pins count after second roll can't be more than unfallen pins");
        }

        public int GetScore()
        {
            return _score.Sum();
        }
    }

    [TestFixture]
    public class Game_should : ReportingTest<Game_should>
    {
        private Game _game;

        [SetUp]
        public void SetUp()
        {
            _game = new Game();
        }


        [Test]
        public void HaveZeroScore_BeforeAnyRolls()
        {
            _game.GetScore().Should().Be(0);
        }

        [Test]
        public void HaveNonZeroScore_AfterFirstRoll()
        {
            _game.Roll(5);

            _game.GetScore().Should().Be(5);
        }

        [Test]
        public void Throws_ArgumentException_IfRollPinsCountMoreThanTen()
        {
            Action action = () => _game.Roll(11);

            action.Should().Throw<ArgumentException>().WithMessage("Pins count can't be more than ten");
        }

        [Test]
        public void Throws_ArgumentException_IfRollPinsCountIsNegative()
        {
            Action action = () => _game.Roll(-1);

            action.Should().Throw<ArgumentException>().WithMessage("Pins count can't be negative");
        }

        [Test]
        public void HaveNonZeroScore_AfterSecondRoll()
        {
           _game.Roll(8);
           _game.Roll(2);
           _game.GetScore().Should().Be(10);
        }

        [Test]
        public void Throws_ArgumentException_IfPinsCountAfterSecondRollMoreThanUnfallenPins()
        {
            _game.Roll(8);

            Action action = () => _game.Roll(8);

            action.Should().Throw<ArgumentException>().WithMessage("Pins count after second roll can't be more than unfallen pins");
        }


        [Test]
        public void GivesBonusScore_AfterSpare()
        {
            _game.Roll(8);
            _game.Roll(2);
            _game.Roll(5);
            _game.GetScore().Should().Be(20);
        }
    }
}
