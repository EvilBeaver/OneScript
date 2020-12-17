/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Security.Cryptography;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Hash
{
    [SystemEnum("ХешФункция", "HashFunction")]
    public class HashFunctionEnum : EnumerationContext
    {
        const string MD5 = "MD5";
        const string SHA1 = "SHA1";
        const string SHA256 = "SHA256";
        const string SHA384 = "SHA384";
        const string SHA512 = "SHA512";
        const string CRC32 = "CRC32";


        [EnumValue(MD5)]
        public EnumerationValue Md5
        {
            get
            {
                return this[MD5];
            }
        }


        [EnumValue(SHA1)]
        public EnumerationValue Sha1
        {
            get
            {
                return this[SHA1];
            }
        }


        [EnumValue(SHA256)]
        public EnumerationValue Sha256
        {
            get
            {
                return this[SHA256];
            }
        }


        [EnumValue(SHA384)]
        public EnumerationValue Sha384
        {
            get
            {
                return this[SHA384];
            }
        }


        [EnumValue(SHA512)]
        public EnumerationValue Sha512
        {
            get
            {
                return this[SHA512];
            }
        }


        [EnumValue(CRC32)]
        public EnumerationValue Crc32
        {
            get
            {
                return this[CRC32];
            }
        }



        private HashFunctionEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }


        public static HashFunctionEnum CreateInstance(ITypeManager typeManager)
        {
            return EnumContextHelper.CreateEnumInstance<HashFunctionEnum>(typeManager,
                (t, v) => new HashFunctionEnum(t, v));
        }

        public static HashAlgorithm GetProvider(IValue provider)
        {
            if (provider.DataType != DataType.GenericValue)
                throw RuntimeException.InvalidArgumentType();

            var neededProvider = provider.GetRawValue() as SelfAwareEnumValue<HashFunctionEnum>;
            if (neededProvider == null)
                throw RuntimeException.InvalidArgumentType();

            var algName = neededProvider.AsString();
            if (algName == "CRC32")
                return new Crc32();

            var ret = HashAlgorithm.Create(algName);
            if (ret == null)
                throw RuntimeException.InvalidArgumentType();
            return ret;

        }
    }
}
