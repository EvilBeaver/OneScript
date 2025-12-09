/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Security.Tokens
{
    /// <summary>
    /// Описывает структуру токена доступа в формате JSON Web Token.
    /// </summary>
    [ContextClass("ТокенДоступа", "AccessToken")]
    public class AccessTokenContext : AutoContext<AccessTokenContext>, IDisposable
    {
        private int? _lifeTime;
        private int? _creationTime;
        private string _tokenId;
        private string _userMatchingKey;
        private string _issuer;
        private string _token;
        private bool _isSigned = false;
        private RSA _rsa;
        private ECDsa _ecdsa;
        private bool _disposed = false;

        /// <summary>
        /// Определяет время жизни токена в секундах.
        /// Устанавливает значение для ключа 'exp' равное сумме времени создания и времени жизни.
        /// </summary>
        /// <value>Число (Number)</value>
        [ContextProperty("ВремяЖизни", "LifeTime")]
        public int LifeTime
        {
            get => _lifeTime ?? 0; 
            set => _lifeTime = value;
        }

        /// <summary>
        /// Числовое значение времени создания токена доступа в формате UnixTime (количество секунд, прошедших с
        /// полуночи 01.01.1970). Соответствует ключам 'iat' и 'nbf'.
        /// </summary>
        /// <value>Число (Number)</value>
        [ContextProperty("ВремяСоздания", "CreationTime")]
        public int CreationTime
        {
            get => _creationTime ?? 0;
            set => _creationTime = value;
        }

        /// <summary>
        /// Заголовки токена доступа.
        /// </summary>
        /// <value>Соответствие (Map)</value>
        [ContextProperty("Заголовки", "Headers")]
        public MapImpl Headers { get; set; }

        /// <summary>
        /// Идентификатор токена.
        /// </summary>
        /// <value>Строка (String)</value>
        [ContextProperty("Идентификатор", "ID")]
        public string TokenId
        {
            get => _tokenId ?? ""; 
            set => _tokenId = value;
        }

        /// <summary>
        /// Ключ сопоставления пользователя. Соответствует ключу 'sub' токена доступа.
        /// </summary>
        /// <value>Строка (String)</value>
        [ContextProperty("КлючСопоставленияПользователя", "UserMatchingKey")]
        public string UserMatchingKey
        {
            get => _userMatchingKey ?? ""; 
            set => _userMatchingKey = value;
        }

        /// <summary>
        /// Полезная нагрузка токена доступа.
        /// </summary>
        /// <value>Соответствие (Map)</value>
        [ContextProperty("ПолезнаяНагрузка", "Payload")]
        public MapImpl Payload { get; set; }

        /// <summary>
        /// Массив строк, который содержит идентификаторы получателей токена. Соответствует ключу 'aud'.
        /// </summary>
        /// <value>Массив (Array)</value>
        [ContextProperty("Получатели", "Recipients")]
        public ArrayImpl Recipients { get; set; }

        /// <summary>
        /// Идентификатор эмитента, выпустившего токен. Соответствует ключу 'iss'.
        /// </summary>
        /// <value>Строка (String)</value>
        [ContextProperty("Эмитент", "Issuer")]
        public string Issuer
        {
            get => _issuer ?? ""; 
            set => _issuer = value;
        }

        /// <summary>
        /// Добавляет токену доступа подпись по указанному в параметрах алгоритму.
        /// </summary>
        /// <param name="algorithm">Алгоритм подписи токена доступа.</param>
        /// <param name="secretKey">Информация о ключе, который используется для формирования подписи в формате PEM. 
        /// Данный параметр является необязательным только, если не указан алгоритм подписи.</param>
        [ContextMethod("Подписать", "Sign")]
        public void Sign(IBslProcess process, AccessTokenSignAlgorithmEnum algorithm, string secretKey = "")
        {
            CreateToken(process, algorithm, secretKey);
            _isSigned = true;
        }

        private void CreateUnsignedToken(IBslProcess process)
        {
            CreateToken(process, AccessTokenSignAlgorithmEnum.None);
        }

        private void CreateToken(IBslProcess process, AccessTokenSignAlgorithmEnum algorithm, string secretKey = "")
        {
            try
            {
                var header = CreateJwtHeader(process, algorithm, secretKey);
                var payload = CreateJwtPayload(process);
                var jwtToken = new JwtSecurityToken(header, payload);
            
                var tokenHandler = new JwtSecurityTokenHandler
                {
                    SetDefaultTimesOnTokenCreation = false
                };
                
                _token = tokenHandler.WriteToken(jwtToken);
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException($"Ошибка при создании токена: {ex.Message}", ex);
            }
        }
   
        private JwtHeader CreateJwtHeader(IBslProcess process, AccessTokenSignAlgorithmEnum algorithm, string secretKey)
        {
            JwtHeader header;

            if (algorithm == AccessTokenSignAlgorithmEnum.None)
            {
                header = new JwtHeader();
                header["alg"] = "none";
            }
            else
            {
                var signingCredentials = GetSigningCredentials(algorithm, secretKey);
                header = new JwtHeader(signingCredentials);
            }
            
            if (Headers != null)
            {
                foreach (var headerItem in Headers)
                {
                    if (headerItem.Key.SystemType != BasicTypes.String)
                        throw RuntimeException.InvalidArgumentType();

                    var key = headerItem.Key.ToString();
                    var value = headerItem.Value.AsString(process);
                
                    if(!String.IsNullOrEmpty(key))
                        header[key] = value;
                }
            }

            return header;
        }
        
        private JwtPayload CreateJwtPayload(IBslProcess process)
        {
            var payload = new JwtPayload();
            
            AddStandardClaimsToPayload(payload);
            AddAudienceToPayload(process, payload);
            AddCustomClaimsToPayload(process, payload);

            return payload;
        }
        
        private void AddStandardClaimsToPayload(JwtPayload payload)
        {
            if (_issuer != null)
                payload[JwtRegisteredClaimNames.Iss] = _issuer;

            if (CreationTime != 0)
            {
                payload[JwtRegisteredClaimNames.Iat] = CreationTime;
                payload[JwtRegisteredClaimNames.Nbf] = CreationTime;
            }

            if (CreationTime != 0 || LifeTime != 0)
            {
                int expires = CreationTime + LifeTime;
                payload[JwtRegisteredClaimNames.Exp] = expires;
            }

            if (_tokenId != null)
                payload[JwtRegisteredClaimNames.Jti] = _tokenId;
                   
            if(_userMatchingKey != null)
                payload[JwtRegisteredClaimNames.Sub] = _userMatchingKey;          
        }
        
        private void AddAudienceToPayload(IBslProcess process, JwtPayload payload)
        {
            if (Recipients == null || Recipients.Count() == 0)
                return;

            if (Recipients.Count() == 1)
            {
                payload[JwtRegisteredClaimNames.Aud] = Recipients[0].AsString(process);
            }
            else
            {
                var recipientsStrings = new List<string>();
                foreach (var recipient in Recipients)
                {
                    if (recipient.SystemType != BasicTypes.String)
                        throw RuntimeException.InvalidArgumentType();

                    recipientsStrings.Add(recipient.ToString());
                }

                payload[JwtRegisteredClaimNames.Aud] = recipientsStrings;
            }
        }

        private void AddCustomClaimsToPayload(IBslProcess process, JwtPayload payload)
        {
            if (Payload == null || Payload.Count() == 0)
                return;

            foreach (var payloadItem in Payload)
            {
                if (payloadItem.Key.SystemType != BasicTypes.String)
                    throw RuntimeException.InvalidArgumentType();

                var key = payloadItem.Key.ToString();
                var value = ConvertToClrObject(process, payloadItem.Value);
                
                if(!String.IsNullOrEmpty(key) && value != null)
                    payload[key] = value;
            }
        }
        
        private SigningCredentials GetSigningCredentials(AccessTokenSignAlgorithmEnum algorithm, string secretKey)
        {
            if (algorithm == AccessTokenSignAlgorithmEnum.None)
                return null;

            if (string.IsNullOrEmpty(secretKey))
                throw new ArgumentException("Ключ подписи не может быть пустым для выбранного алгоритма");

            var key = GetSigningKey(algorithm, secretKey);
            return new SigningCredentials(key, GetSecurityAlgorithm(algorithm));
        }
        
        private SecurityKey GetSigningKey(AccessTokenSignAlgorithmEnum algorithm, string secretKey)
        {
            return algorithm switch
            {
                // Симметричные алгоритмы (HMAC)
                AccessTokenSignAlgorithmEnum.HS256 => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                AccessTokenSignAlgorithmEnum.HS384 => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                AccessTokenSignAlgorithmEnum.HS512 => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),

                // Асимметричные алгоритмы (RSA)
                AccessTokenSignAlgorithmEnum.RS256 => CreateRsaSecurityKey(secretKey),
                AccessTokenSignAlgorithmEnum.RS384 => CreateRsaSecurityKey(secretKey),
                AccessTokenSignAlgorithmEnum.RS512 => CreateRsaSecurityKey(secretKey),
                AccessTokenSignAlgorithmEnum.PS256 => CreateRsaSecurityKey(secretKey),
                AccessTokenSignAlgorithmEnum.PS384 => CreateRsaSecurityKey(secretKey),
                AccessTokenSignAlgorithmEnum.PS512 => CreateRsaSecurityKey(secretKey),

                // Эллиптические кривые (ECDSA)
                AccessTokenSignAlgorithmEnum.ES256 => CreateEcdsaSecurityKey(secretKey),
                AccessTokenSignAlgorithmEnum.ES384 => CreateEcdsaSecurityKey(secretKey),
                AccessTokenSignAlgorithmEnum.ES512 => CreateEcdsaSecurityKey(secretKey),

                _ => throw new ArgumentException($"Неподдерживаемый алгоритм для ключа: {algorithm}")
            };
        }
        
        private SecurityKey CreateRsaSecurityKey(string secretKey)
        {
            _rsa?.Dispose();
            _rsa = RSA.Create();
            try
            {
                _rsa.ImportFromPem(secretKey);
                return new RsaSecurityKey(_rsa);
            }
            catch (Exception ex)
            {
                _rsa.Dispose();
                _rsa = null;
                throw new ArgumentException($"Ошибка при создании RSA ключа: {ex.Message}", ex);
            }
        }
        
        private SecurityKey CreateEcdsaSecurityKey(string secretKey)
        {
            _ecdsa?.Dispose();
            _ecdsa = ECDsa.Create();
            try
            {
                _ecdsa.ImportFromPem(secretKey);
                return new ECDsaSecurityKey(_ecdsa);
            }
            catch (Exception ex)
            {
                _ecdsa.Dispose();
                _ecdsa = null;
                throw new ArgumentException($"Ошибка при создании ECDSA ключа: {ex.Message}", ex);
            }
        }
        
        private string GetSecurityAlgorithm(AccessTokenSignAlgorithmEnum algorithm)
        {
            return algorithm switch
            {
                // HMAC алгоритмы
                AccessTokenSignAlgorithmEnum.HS256 => SecurityAlgorithms.HmacSha256,
                AccessTokenSignAlgorithmEnum.HS384 => SecurityAlgorithms.HmacSha384,
                AccessTokenSignAlgorithmEnum.HS512 => SecurityAlgorithms.HmacSha512,
                
                // RSA алгоритмы
                AccessTokenSignAlgorithmEnum.RS256 => SecurityAlgorithms.RsaSha256,
                AccessTokenSignAlgorithmEnum.RS384 => SecurityAlgorithms.RsaSha384,
                AccessTokenSignAlgorithmEnum.RS512 => SecurityAlgorithms.RsaSha512,
                
                // RSA-PSS алгоритмы
                AccessTokenSignAlgorithmEnum.PS256 => SecurityAlgorithms.RsaSsaPssSha256,
                AccessTokenSignAlgorithmEnum.PS384 => SecurityAlgorithms.RsaSsaPssSha384,
                AccessTokenSignAlgorithmEnum.PS512 => SecurityAlgorithms.RsaSsaPssSha512,
                
                // ECDSA алгоритмы
                AccessTokenSignAlgorithmEnum.ES256 => SecurityAlgorithms.EcdsaSha256,
                AccessTokenSignAlgorithmEnum.ES384 => SecurityAlgorithms.EcdsaSha384,
                AccessTokenSignAlgorithmEnum.ES512 => SecurityAlgorithms.EcdsaSha512,
                
                _ => throw new ArgumentException($"Неподдерживаемый алгоритм: {algorithm}")
            };
        }
        
        private object ConvertToClrObject(IBslProcess process, IValue value)
        {
            if (value == null)
                return null;

            switch (value)
            {
                case ArrayImpl array:
                case FixedArrayImpl fixedArray:
                    var list = new List<object>();
                    foreach (var item in (IEnumerable<IValue>)value)
                    {
                        list.Add(ConvertToClrObject(process, item));
                    }
                    return list; 
                case StructureImpl structure:
                case FixedStructureImpl fixedStructure:
                case MapImpl map:
                case FixedMapImpl fixedMap:
                    var dict = new Dictionary<string, object>();
                    foreach (var item in (IEnumerable<KeyAndValueImpl>)value)
                    {
                        var key = item.Key.AsString(process);
                        dict[key] = ConvertToClrObject(process, item.Value);
                    }
                    return dict; 
                default:
                    var unwarpedValue = value.UnwrapToClrObject() ?? "";
                    return unwarpedValue.GetType().IsValueType ? unwarpedValue : value.AsString(process);
            }
        }
        
        [ScriptConstructor(Name = "По умолчанию")]
        public static AccessTokenContext Constructor()
        {
            return new AccessTokenContext();
        }
         
        [ScriptConstructor(Name = "По заголовкам и полезной нагрузке")]
        public static AccessTokenContext Constructor(MapImpl headers, MapImpl payload)
        {
            return new AccessTokenContext(headers, payload);
        }

        public override string ToString(IBslProcess process)
        {
            if (_isSigned)
                return _token;

            CreateUnsignedToken(process);
            
            return _token;
        }

        private AccessTokenContext()
        {
            Headers = new MapImpl();
            Payload = new MapImpl();
            Recipients = new ArrayImpl();
        }
        
        private AccessTokenContext(MapImpl headers, MapImpl payload) : this()
        {
            Headers = headers;
            Payload = payload;
        }
        
        public void Dispose()
        {
            if (_disposed)
                return;
            
            _rsa?.Dispose();
            _ecdsa?.Dispose();
            _rsa = null;
            _ecdsa = null;

            _disposed = true;
        }
    }
}
