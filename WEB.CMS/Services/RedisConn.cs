using Entities.ViewModels.Car;
using StackExchange.Redis;
using System.Text.Json;
using Utilities;

namespace WEB.CMS.Services
{
    public class RedisConn
    {

        private readonly string _redisHost;
        private readonly int _redisPort;
        // private readonly int _db_index;        

        private ConnectionMultiplexer _redis;
        public RedisConn(IConfiguration config)
        {
            _redisHost = config["Redis:Host"];
            _redisPort = Convert.ToInt32(config["Redis:Port"]);
            // _db_index = Convert.ToInt32(config["Redis:Database:db_product"]);            
        }
        public async Task SubscribeAsync(string channel, Action<RegistrationRecord> onMessage)
        {
            var sub = _redis.GetSubscriber();
            await sub.SubscribeAsync(channel, (ch, msg) =>
            {
                try
                {
                    var record = JsonSerializer.Deserialize<RegistrationRecord>(msg!);
                    if (record != null)
                    {
                        onMessage?.Invoke(record);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.InsertLogTelegram("Redis Subscribe Deserialize Error: " + ex.Message);
                }
            });
        }
        public void Connect()
        {
            try
            {
                var configString = $"{_redisHost}:{_redisPort},connectRetry=5,allowAdmin=true";
                _redis = ConnectionMultiplexer.Connect(configString);
            }
            catch (RedisConnectionException err)
            {
                LogHelper.InsertLogTelegram("Redis Connection Error: " + err.Message);
                // throw err;
            }
            // Log.Debug("Connected to Redis");
        }

        public void Set(string key, string value, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            db.StringSet(key, value);
        }
        public void Set(string key, string value, DateTime expires, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            var expiryTimeSpan = expires.Subtract(DateTime.Now);

            db.StringSet(key, value, expiryTimeSpan);
        }

        public async Task<string> GetAsync(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return await db.StringGetAsync(key);
        }
        public string Get(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return db.StringGet(key);
        }

        public string GetNoAsync(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            return db.StringGet(key);
        }

        public async void clear(string key, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            await db.KeyDeleteAsync(key);
        }
        public async void FlushDatabaseByIndex(int db_index)
        {
            await _redis.GetServer(_redisHost, _redisPort).FlushDatabaseAsync(db_index);
        }

        public async Task DeleteCacheByKeyword(string keyword, int db_index)
        {
            var db = _redis.GetDatabase(db_index);
            var server = _redis.GetServer(_redisHost, _redisPort);
            var keys = server.Keys(db_index, pattern: "*" + keyword + "*").ToList();
            foreach (var key in keys)
            {
                try
                {
                    await db.KeyDeleteAsync(key);
                }
                catch { }
            }
        }
        // ===============================
        // Pub/Sub cho realtime
        // ===============================

        /// <summary>
        /// Subscribe channel và xử lý message realtime
        /// </summary>

        public async Task PublishAsync(string channel, RegistrationRecord record)
        {
            var sub = _redis.GetSubscriber();
            var json = JsonSerializer.Serialize(record);
            await sub.PublishAsync(channel, json);
        }

    }
}
