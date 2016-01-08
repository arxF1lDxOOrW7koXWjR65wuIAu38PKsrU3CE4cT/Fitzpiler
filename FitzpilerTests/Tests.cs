﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Fitzpiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Fitzpiler.Tests
{
    [TestClass()]
    public class Tests
    {
        Tokenizer t;
        [TestInitialize()]
        public void TestInitialize()
        {
            string[] program = File.ReadAllLines(@"testfile1.txt");
            t = new Tokenizer(program);
        }
        [TestMethod()]
        public void TokenizerTest()
        {
            Token token;
            Queue<Token> queue = new Queue<Token>();
            while ((token = t.Next()) != null)
            {
                queue.Enqueue(token);
            }
            Assert.AreEqual<int>(queue.Count, 145);
        }

        [TestMethod]
        public void ParserTest()
        {
            Parser parser = new Parser(new Scanner(File.ReadAllLines(@"testfile1.txt")));
            Assert.IsNotNull(parser.program);
            Assert.IsTrue(parser.program.Verify());
        }
    }
}