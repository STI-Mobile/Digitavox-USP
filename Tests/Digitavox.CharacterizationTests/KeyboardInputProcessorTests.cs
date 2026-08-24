using Digitavox.Helpers;
using Digitavox.Models;
using Digitavox.Presentation.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.CharacterizationTests;

[TestClass]
public class KeyboardInputProcessorTests
{
    [TestMethod]
    public async Task Dead_key_state_is_isolated_between_screen_input_processors()
    {
        FingerMapping mapping = new(new TestAppEnvironment());
        await mapping.InitializeAsync();
        KeyboardInputProcessor firstScreen = new(mapping);
        KeyboardInputProcessor secondScreen = new(mapping);

        Assert.IsNull(firstScreen.KeyUp(71, modifiers: 0).code);
        Assert.AreEqual("a", secondScreen.KeyUp(29, modifiers: 0).code);
        Assert.AreEqual("á", firstScreen.KeyUp(29, modifiers: 0).code);
    }

    [TestMethod]
    public async Task Key_down_and_key_up_are_normalized_by_one_pipeline()
    {
        FingerMapping mapping = new(new TestAppEnvironment());
        await mapping.InitializeAsync();
        KeyboardInputProcessor processor = new(mapping);

        Assert.IsTrue(processor.KeyDown(29));
        FingerMappingBean result = processor.KeyUp(29, modifiers: 0);

        Assert.AreEqual("a", result.code);
        Assert.AreEqual("a mínimo esquerdo", result.speak);
    }
}
