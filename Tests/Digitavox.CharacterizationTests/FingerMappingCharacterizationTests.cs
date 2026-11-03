using Digitavox.Helpers;
using Digitavox.Models;
using Digitavox.Core.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Digitavox.CharacterizationTests;

[TestClass]
public class FingerMappingCharacterizationTests
{
    [TestMethod]
    public async Task Android_key_code_maps_to_common_code_and_finger_guidance()
    {
        FingerMapping mapping = new(new TestAppEnvironment());
        await mapping.InitializeAsync();

        FingerMappingBean result = mapping.MapKey(29, modifiers: 0, pressedKeys: new List<string>());

        Assert.AreEqual("a", mapping.mapKeyCode(29));
        Assert.AreEqual("a", result.code);
        Assert.AreEqual("a", result.show);
        Assert.AreEqual("a mínimo esquerdo", result.speak);
        Assert.AreEqual("mínimo esquerdo", mapping.Code2Finger("a"));
    }

    [TestMethod]
    public async Task Shift_and_caps_lock_produce_uppercase_mapping()
    {
        FingerMapping mapping = new(new TestAppEnvironment());
        await mapping.InitializeAsync();

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
    public async Task Acute_dead_key_is_combined_with_the_following_vowel()
    {
        FingerMapping mapping = new(new TestAppEnvironment());
        await mapping.InitializeAsync();

        FingerMappingBean deadKey = mapping.MapKey(71, modifiers: 0, pressedKeys: new List<string>());
        FingerMappingBean accentedLetter = mapping.MapKey(29, modifiers: 0, pressedKeys: new List<string>());

        Assert.IsNull(deadKey.code);
        Assert.AreEqual("á", accentedLetter.code);
    }

    [TestMethod]
    [DataRow(AppPlatformKind.Ios, 4)]
    [DataRow(AppPlatformKind.MacCatalyst, 4)]
    [DataRow(AppPlatformKind.Windows, 65)]
    public async Task Platform_key_codes_are_normalized_to_the_android_mapping(AppPlatformKind platform, int nativeCode)
    {
        FingerMapping mapping = new(new TestAppEnvironment(platform));
        await mapping.InitializeAsync();

        Assert.AreEqual("a", mapping.mapKeyCode(nativeCode));
        Assert.AreEqual("a", mapping.MapKey(nativeCode, 0, new List<string>()).code);
    }
}
