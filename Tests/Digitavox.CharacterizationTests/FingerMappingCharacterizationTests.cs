using Digitavox.Helpers;
using Digitavox.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.CharacterizationTests;

[TestClass]
public class FingerMappingCharacterizationTests
{
    [TestInitialize]
    public void SetUp()
    {
        DVDevice.Platform = TestPlatform.Android;
    }

    [TestMethod]
    public void Android_key_code_maps_to_common_code_and_finger_guidance()
    {
        FingerMapping mapping = new();

        FingerMappingBean result = mapping.MapKey(29, modifiers: 0, pressedKeys: new List<string>());

        Assert.AreEqual("a", mapping.mapKeyCode(29));
        Assert.AreEqual("a", result.code);
        Assert.AreEqual("a", result.show);
        Assert.AreEqual("a mínimo esquerdo", result.speak);
        Assert.AreEqual("mínimo esquerdo", mapping.Code2Finger("a"));
    }

    [TestMethod]
    public void Shift_and_caps_lock_produce_uppercase_mapping()
    {
        FingerMapping mapping = new();

        FingerMappingBean shifted = mapping.MapKey(
            29,
            modifiers: (int)Modifier.Shift,
            pressedKeys: new List<string>());
        FingerMappingBean capsLocked = mapping.MapKey(
            29,
            modifiers: (int)Modifier.CapsLock,
            pressedKeys: new List<string>());

        Assert.AreEqual("A", shifted.code);
        Assert.AreEqual("A", capsLocked.code);
    }

    [TestMethod]
    public void Acute_dead_key_is_combined_with_the_following_vowel()
    {
        FingerMapping mapping = new();

        FingerMappingBean deadKey = mapping.MapKey(71, modifiers: 0, pressedKeys: new List<string>());
        FingerMappingBean accentedLetter = mapping.MapKey(29, modifiers: 0, pressedKeys: new List<string>());

        Assert.IsNull(deadKey.code);
        Assert.AreEqual("á", accentedLetter.code);
    }

    [TestMethod]
    [DataRow(TestPlatform.Ios, 4)]
    [DataRow(TestPlatform.Mac, 4)]
    [DataRow(TestPlatform.Windows, 65)]
    public void Platform_key_codes_are_normalized_to_the_android_mapping(TestPlatform platform, int nativeCode)
    {
        DVDevice.Platform = platform;
        FingerMapping mapping = new();

        Assert.AreEqual("a", mapping.mapKeyCode(nativeCode));
        Assert.AreEqual("a", mapping.MapKey(nativeCode, 0, new List<string>()).code);
    }
}
