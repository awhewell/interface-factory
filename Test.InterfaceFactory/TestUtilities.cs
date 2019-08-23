// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using Moq;
using System.Linq.Expressions;
using InterfaceFactory;
using System.Collections.Specialized;

namespace Test.InterfaceFactory
{
    /// <summary>
    /// A collection of static methods that cover common aspects of testing objects.
    /// </summary>
    public static class TestUtilities
    {
        /// <summary>
        /// Throws an assertion if the type passed across is not an exception type, does not have the
        /// correct constructors or is not serialisable.
        /// </summary>
        /// <param name="exceptionType"></param>
        public static void AssertIsException(Type exceptionType)
        {
            Assert.IsNotNull(exceptionType);
            Assert.IsTrue(typeof(Exception).IsAssignableFrom(exceptionType), "Exception does not inherit from System.Exception");

            if(exceptionType.GetCustomAttributes(typeof(SerializableAttribute), false) == null) {
                Assert.Fail("The exception is not serializable");
            }
            var serializeCtor = exceptionType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(SerializationInfo), typeof(StreamingContext) },
                new ParameterModifier[0]
            );
            Assert.IsNotNull(serializeCtor);

            var defaultCtor = exceptionType.GetConstructor(new Type[] {});
            var stringCtor = exceptionType.GetConstructor(new Type[] {typeof(String)});
            var innerExceptionCtor = exceptionType.GetConstructor(new Type[] {typeof(String), typeof(Exception)});

            Assert.IsNotNull(defaultCtor);
            Assert.IsNotNull(stringCtor);
            Assert.IsNotNull(innerExceptionCtor);

            var defaultException = (Exception)Activator.CreateInstance(exceptionType);
            Assert.IsNull(defaultException.InnerException);

            var expectedText = "My Message TeXT";
            var stringException = (Exception)stringCtor.Invoke(
                new object[]{
                    expectedText
                }
            );
            Assert.IsTrue(stringException.Message.IndexOf(expectedText) > -1);
            Assert.IsNull(stringException.InnerException);

            var inner = new Exception();
            var innerException = (Exception)innerExceptionCtor.Invoke(
                new object[]{
                    expectedText,
                    inner
                }
            );
            Assert.IsTrue(innerException.Message.IndexOf(expectedText) > -1);
            Assert.AreSame(inner, innerException.InnerException);
        }
    }
}
