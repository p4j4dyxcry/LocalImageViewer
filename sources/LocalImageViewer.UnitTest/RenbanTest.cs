using System;
using LocalImageViewer.Service;
using Xunit;

namespace LocalImageViewer.UnitTest
{
    public class RenbanTest
    {
        [Theory]
        [InlineData("hoge",-1)]
        [InlineData("test32",32)]
        [InlineData("abc100.jpg",100)]
        [InlineData("38-10-5.jpg",5)]
        public void FindLastNumberTest(string input , int expeceted)
        {
            Assert.Equal(expeceted,RenbanHelper.FindLastNumber(input));
        }
        
        [Theory]
        [InlineData("hoge/abc","hoge/abc")]
        [InlineData("test32","test*")]
        [InlineData("abc100.jpg","abc*.jpg")]
        [InlineData("38-10-5.jpg","38-10-*.jpg")]
        public void ReplaceLastValueTest(string input , string expeceted)
        {
            Assert.Equal(expeceted,RenbanHelper.ReplaceLastNumber(input,"*"));
        }
    }
}