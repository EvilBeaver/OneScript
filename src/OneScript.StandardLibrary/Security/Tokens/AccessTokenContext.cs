/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using OneScript.Execution;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections;
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
            var claims = new List<Claim>();

            AddStandardClaims(claims);
            AddAudienceToClaims(process, claims);
            AddPayloadToClaims(process, claims);
            
            var tokenHandler = new JwtSecurityTokenHandler
            {
                SetDefaultTimesOnTokenCreation = false
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                AdditionalInnerHeaderClaims = GetHeaderClaims(process),
                SigningCredentials = GetSigningCredentials(algorithm, secretKey)
            };

            try
            {
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                _token = tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException($"Ошибка при создании токена: {ex.Message}", ex);
            }
        }

        private void AddStandardClaims(List<Claim> claims)
        {
            if(_issuer != null)
                claims.Add(new Claim(JwtRegisteredClaimNames.Iss, _issuer));

            if (CreationTime != 0)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Iat, CreationTime.ToString(), ClaimValueTypes.Integer64));
                claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, CreationTime.ToString(), ClaimValueTypes.Integer64));
            }

            if (CreationTime != 0 || LifeTime != 0)
            {
                int expires = CreationTime + LifeTime;
                claims.Add(new Claim(JwtRegisteredClaimNames.Exp, expires.ToString(), ClaimValueTypes.Integer64));
            }
                
            if(_tokenId != null)
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, _tokenId));
                   
            if(_userMatchingKey != null)
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, _userMatchingKey));          
        }
        
        private void AddAudienceToClaims(IBslProcess process, List<Claim> claims)
        {
            if (Recipients == null || Recipients.Count() == 0)
                return;
            
            foreach (var recipient in Recipients)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Aud, recipient.AsString(process)));
            }
        }

        private void AddPayloadToClaims(IBslProcess process, List<Claim> claims)
        {
            if (Payload == null || Payload.Count() == 0)
                return;
            
            foreach (var payloadItem in Payload)
            {
                var key = payloadItem.Key?.AsString(process);
                var value = payloadItem.Value?.AsString(process);

                if(!String.IsNullOrEmpty(key) && value != null)
                    claims.Add(new Claim(key, value));
            }
        }

        private Dictionary<string, object> GetHeaderClaims(IBslProcess process)
        {
            var headerClaims = new Dictionary<string, object>();
            
            if (Headers == null || Headers.Count() == 0)
                return headerClaims;

            foreach (var headerItem in Headers)
            {
                var key = headerItem.Key.AsString(process);
                var value = headerItem.Value.AsString(process);
                
                if(!String.IsNullOrEmpty(key))
                    headerClaims.Add(key, value);
            }

            return headerClaims;
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
