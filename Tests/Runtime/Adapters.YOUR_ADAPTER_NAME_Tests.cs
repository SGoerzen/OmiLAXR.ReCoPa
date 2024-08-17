using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using UnityEditor;

namespace OmiLAXR.Modules.ReCoPa.Tests
{
    public class OmiLAXR_YOUR_ADAPTER_NAME_Tests
    {
        [SetUp]
        public void Setup()
        {
        }
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator DataPassedFullPipeline()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }// A Test behaves as an ordinary method
        [Test]
        public void TestPrefab()
        {
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}