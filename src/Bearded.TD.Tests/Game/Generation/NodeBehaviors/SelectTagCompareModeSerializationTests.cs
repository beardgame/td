using System.Text.Json;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;
using static Bearded.TD.Game.Generation.Semantic.NodeBehaviors.SelectTag;
using static Bearded.TD.Game.Generation.Semantic.NodeBehaviors.SelectTag.CompareMode;

namespace Bearded.TD.Tests.Game.Generation.NodeBehaviors
{
    public sealed class SelectTagCompareModeSerializationTests
    {
        private sealed class Model
        {
            public CompareMode G;
            public CompareMode GE;
            public CompareMode L;
            public CompareMode LE;

            public void AssertCorrectValues()
            {
                G.Should().Be(Greater);
                GE.Should().Be(GreaterOrEqual);
                L.Should().Be(Less);
                LE.Should().Be(LessOrEqual);
            }
        }

        [Fact]
        public void JsonNetDeserializesOperatorValues()
        {
            var model = JsonConvert.DeserializeObject<Model>(@"{
'g' : '>',
'ge' : '>=',
'l' : '<',
'le' : '<='
}");

            model.AssertCorrectValues();
        }

        [Fact]
        public void JsonNetDeserializesSpelledOutValues()
        {
            var model = JsonConvert.DeserializeObject<Model>($@"{{
'g' : '{nameof(Greater)}',
'ge' : '{nameof(GreaterOrEqual)}',
'l' : '{nameof(Less)}',
'le' : '{nameof(LessOrEqual)}'
}}");

            model.AssertCorrectValues();
        }

        [Fact]
        public void JsonNetSerializesAndDeserializesAllValues()
        {
            var originalModel = new Model
            {
                G = Greater,
                GE = GreaterOrEqual,
                L = Less,
                LE = LessOrEqual
            };

            var json = JsonConvert.SerializeObject(originalModel);

            var model = JsonConvert.DeserializeObject<Model>(json);

            model.AssertCorrectValues();
        }
    }
}
