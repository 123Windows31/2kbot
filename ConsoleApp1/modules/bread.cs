// 2kbot，一款用C#编写的基于mirai和mirai.net的自由机器人软件
// Copyright(C) 2022 Abjust 版权所有。

// 本程序是自由软件：你可以根据自由软件基金会发布的GNU Affero通用公共许可证的条款，即许可证的第3版或（您选择的）任何后来的版本重新发布它和/或修改它。。

// 本程序的发布是希望它能起到作用。但没有任何保证；甚至没有隐含的保证。本程序的分发是希望它是有用的，但没有任何保证，甚至没有隐含的适销对路或适合某一特定目的的保证。 参见 GNU Affero通用公共许可证了解更多细节。

// 您应该已经收到了一份GNU Affero通用公共许可证的副本。 如果没有，请参见<https://www.gnu.org/licenses/>。

// 致所有构建及修改2kbot代码片段的用户：作者（Abjust）并不承担构建2kbot代码片段（包括修改过的版本）所产生的一切风险，但是用户有权在2kbot的GitHub项目页提出issue，并有权在代码片段修复这些问题后获取这些更新，但是，作者不会对修改过的代码版本做质量保证，也没有义务修正在修改过的代码片段中存在的任何缺陷。

using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using MySql.Data.MySqlClient;

namespace Net_2kBot.Modules
{
    public static class Bread
    {
        public const int breadfactory_maxlevel = 5;
        // 建造面包厂
        public static async void BuildFactory(string group)
        {
            // 连接数据库
            using (var msc = new MySqlConnection(Global.connectstring))
            {
                await msc.OpenAsync();
                MySqlCommand cmd = new()
                {
                    Connection = msc
                };
                // 判断数据是否存在
                cmd.CommandText = $"SELECT COUNT(*) gid FROM bread WHERE gid = {group};";
                int i = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (i == 0)
                {
                    cmd.CommandText = $"INSERT INTO bread (gid) VALUES ({group});";
                    await cmd.ExecuteNonQueryAsync();
                    cmd.CommandText = $"INSERT INTO material (gid) VALUES ({group});";
                    await cmd.ExecuteNonQueryAsync();
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(group, "成功为本群建造面包厂！");
                    }
                    catch
                    {
                        Console.WriteLine("群消息发送失败");
                    }
                }
                else
                {
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(group, "本群已经有面包厂了！");
                    }
                    catch
                    {
                        Console.WriteLine("群消息发送失败");
                    }
                }
            }
        }
        // 给2kbot面包
        public static async void Give(string group, string executor, int number)
        {
            // 连接数据库
            using (var msc = new MySqlConnection(Global.connectstring))
            {
                await msc.OpenAsync();
                MySqlCommand cmd = new()
                {
                    Connection = msc
                };
                cmd.CommandText = $"SELECT * FROM bread WHERE gid = {group};";
                try
                {
                    MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                    await reader.ReadAsync();
                    if (reader.GetInt32("bread_diversity") == 0)
                    {
                        if (number + reader.GetInt32("breads") <= (int)(32 * Math.Pow(4, reader.GetInt32("factory_level") - 1) * Math.Pow(2, reader.GetInt32("storage_upgraded"))))
                        {
                            using (var msc1 = new MySqlConnection(Global.connectstring))
                            {
                                await msc1.OpenAsync();
                                MySqlCommand cmd1 = new()
                                {
                                    Connection = msc1
                                };
                                cmd1.CommandText = $"UPDATE bread SET breads = {reader.GetInt32("breads") + number} WHERE gid = {group};";
                                await cmd1.ExecuteNonQueryAsync();
                                await reader.CloseAsync();
                                cmd.CommandText = $"SELECT * FROM bread WHERE gid = {group};";
                                reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                                await reader.ReadAsync();
                                MessageChain? messageChain = new MessageChainBuilder()
                               .At(executor)
                               .Plain($" 现在库存有 {reader.GetInt32("breads")} 块面包辣！")
                               .Build();
                                try
                                {
                                    await MessageManager.SendGroupMessageAsync(group, messageChain);
                                }
                                catch
                                {
                                    Console.WriteLine("群消息发送失败");
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(group, "抱歉，库存已经满了。。。");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(group, "除非本群供应模式为“单一化供应”，否则你无法给予2kbot面包！");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                    }
                }
                catch
                {
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(group, "本群还没有面包厂！");
                    }
                    catch
                    {
                        Console.WriteLine("群消息发送失败");
                    }
                }
            }
        }
        // 给我面包
        public static async void Get(string group, string executor, int number)
        {
            // 连接数据库
            using (var msc = new MySqlConnection(Global.connectstring))
            {
                await msc.OpenAsync();
                MySqlCommand cmd = new()
                {
                    Connection = msc
                };
                cmd.CommandText = $"SELECT * FROM bread WHERE gid = {group};";
                try
                {
                    MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                    await reader.ReadAsync();
                    if (reader.GetInt32("bread_diversity") != 2)
                    {
                        if (reader.GetInt32("breads") >= number)
                        {
                            if (reader.GetInt32("bread_diversity") == 1)
                            {
                                List<string> bread_types = new()
                            {
                            "🍞",
                            "🥖",
                            "🥐",
                            "🥯",
                            "🍩"
                            };
                                if (number >= bread_types.Count)
                                {
                                    int ExpectedSum = number;
                                    Random rnd = new Random();
                                    int[] fields = new int[bread_types.Count];
                                    int sum = 0;
                                    for (int i = 0; i < fields.Length - 1; i++)
                                    {
                                        fields[i] = rnd.Next(ExpectedSum);
                                        sum += fields[i];
                                    }
                                    int actualSum = sum * fields.Length / (fields.Length - 1);
                                    sum = 0;
                                    for (int i = 0; i < fields.Length - 1; i++)
                                    {
                                        fields[i] = fields[i] * ExpectedSum / actualSum;
                                        sum += fields[i];
                                    }
                                    fields[fields.Length - 1] = ExpectedSum - sum;
                                    string text = "";
                                    for (int i = 0; i < bread_types.Count; i++)
                                    {
                                        if (i == 0)
                                        {
                                            text = $"\n{bread_types[i]}*{fields[i]}";
                                        }
                                        else
                                        {
                                            text += $"\n{bread_types[i]}*{fields[i]}";
                                        }
                                    }
                                    MessageChain? messageChain = new MessageChainBuilder()
                                   .At(executor)
                                   .Plain(text)
                                   .Build();
                                    using (var msc1 = new MySqlConnection(Global.connectstring))
                                    {
                                        await msc1.OpenAsync();
                                        MySqlCommand cmd1 = new()
                                        {
                                            Connection = msc1
                                        };
                                        cmd1.CommandText = $"UPDATE bread SET breads = {reader.GetInt32("breads") - number} WHERE gid = {group};";
                                        await cmd1.ExecuteNonQueryAsync();
                                    }
                                    try
                                    {
                                        await MessageManager.SendGroupMessageAsync(group, messageChain);
                                    }
                                    catch
                                    {
                                        Console.WriteLine("群消息发送失败");
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        await MessageManager.SendGroupMessageAsync(group, $"你请求进货的面包数太少了！（至少要有 {bread_types.Count} 块）");
                                    }
                                    catch
                                    {
                                        Console.WriteLine("群消息发送失败");
                                    }
                                }
                            }
                            else
                            {
                                using (var msc1 = new MySqlConnection(Global.connectstring))
                                {
                                    await msc1.OpenAsync();
                                    MySqlCommand cmd1 = new()
                                    {
                                        Connection = msc1
                                    };
                                    cmd1.CommandText = $"UPDATE bread SET breads = {reader.GetInt32("breads") - number} WHERE gid = {group};";
                                    await cmd1.ExecuteNonQueryAsync();
                                }
                                MessageChain? messageChain = new MessageChainBuilder()
                               .At(executor)
                               .Plain($" 🍞*{number}")
                               .Build();
                                try
                                {
                                    await MessageManager.SendGroupMessageAsync(group, messageChain);
                                }
                                catch
                                {
                                    Console.WriteLine("群消息发送失败");
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(group, "抱歉，面包不够了。。。");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    else
                    {
                        MessageChain? messageChain = new MessageChainBuilder()
                               .At(executor)
                               .Plain($" 🍞*{number}")
                               .Build();
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(group, messageChain);
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                    }
                }
                catch
                {
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(group, "本群还没有面包厂！");
                    }
                    catch
                    {
                        Console.WriteLine("群消息发送失败");
                    }
                }
            }
        }
        // 查询面包库存
        public static async void Query(string group, string executor)
        {
            // 连接数据库
            using (var msc = new MySqlConnection(Global.connectstring))
            {
                await msc.OpenAsync();
                MySqlCommand cmd = new()
                {
                    Connection = msc
                };
                try
                {
                    cmd.CommandText = $"SELECT * FROM bread WHERE gid = {group};";
                    MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                    await reader.ReadAsync();
                    MessageChain? messageChain = new MessageChainBuilder()
                    .At(executor)
                    .Plain($" 现在库存有 {reader.GetInt32("breads")} 块面包，本群面包厂目前最多可储存 {(int)(32 * Math.Pow(4, reader.GetInt32("factory_level") - 1) * Math.Pow(2, reader.GetInt32("storage_upgraded")))} 块面包")
                    .Build();
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(group, messageChain);
                    }
                    catch
                    {
                        Console.WriteLine("群消息发送失败");
                    }
                }
                catch
                {
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(group, "本群还没有面包厂！");
                    }
                    catch
                    {
                        Console.WriteLine("群消息发送失败");
                    }
                }
            }
        }
        // 修改生产模式
        public static async void ChangeMode(string group, int mode)
        {
            // 连接数据库
            using (var msc = new MySqlConnection(Global.connectstring))
            {
                await msc.OpenAsync();
                MySqlCommand cmd = new()
                {
                    Connection = msc
                };
                // 判断数据是否存在
                cmd.CommandText = $"SELECT COUNT(*) gid FROM bread WHERE gid = {group};";
                int i = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (i == 1)
                {
                    cmd.CommandText = $"SELECT * FROM bread WHERE gid = {group};";
                    MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                    await reader.ReadAsync();
                    if (reader.GetInt32("breads") == 0)
                    {
                        switch (mode)
                        {
                            case 2:
                                using (var msc1 = new MySqlConnection(Global.connectstring))
                                {
                                    await msc1.OpenAsync();
                                    MySqlCommand cmd1 = new()
                                    {
                                        Connection = msc1
                                    };
                                    cmd1.CommandText = $"UPDATE bread SET bread_diversity = 2 WHERE gid = {group};";
                                    await cmd1.ExecuteNonQueryAsync();
                                }
                                try
                                {
                                    await MessageManager.SendGroupMessageAsync(group, "已将本群供应模式修改为：无限供应");
                                }
                                catch
                                {
                                    Console.WriteLine("群消息发送失败");
                                }
                                break;
                            case 1:
                                using (var msc1 = new MySqlConnection(Global.connectstring))
                                {
                                    await msc1.OpenAsync();
                                    MySqlCommand cmd1 = new()
                                    {
                                        Connection = msc1
                                    };
                                    cmd1.CommandText = $"UPDATE bread SET bread_diversity = 1 WHERE gid = {group};";
                                    await cmd1.ExecuteNonQueryAsync();
                                }
                                try
                                {
                                    await MessageManager.SendGroupMessageAsync(group, "已将本群供应模式修改为：多样化供应");
                                }
                                catch
                                {
                                    Console.WriteLine("群消息发送失败");
                                }
                                break;
                            case 0:
                                using (var msc1 = new MySqlConnection(Global.connectstring))
                                {
                                    await msc1.OpenAsync();
                                    MySqlCommand cmd1 = new()
                                    {
                                        Connection = msc1
                                    };
                                    cmd1.CommandText = $"UPDATE bread SET bread_diversity = 1 WHERE gid = {group};";
                                    await cmd1.ExecuteNonQueryAsync();
                                }
                                try
                                {
                                    await MessageManager.SendGroupMessageAsync(group, "已将本群供应模式修改为：单一化供应");
                                }
                                catch
                                {
                                    Console.WriteLine("群消息发送失败");
                                }
                                break;
                        }
                    }
                    else
                    {
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(group, "你必须先清空库存，才能修改生产模式！");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                    }
                }
                else
                {
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(group, "本群还没有面包厂！");
                    }
                    catch
                    {
                        Console.WriteLine("群消息发送失败");
                    }
                }
            }
        }
        // 获取经验
        public static async void GetExp(MessageReceiverBase @base)
        {
            if (@base is GroupMessageReceiver receiver)
            {
                // 连接数据库
                using (var msc = new MySqlConnection(Global.connectstring))
                {
                    await msc.OpenAsync();
                    MySqlCommand cmd = new()
                    {
                        Connection = msc
                    };
                    // 判断数据是否存在
                    cmd.CommandText = $"SELECT COUNT(*) gid FROM bread WHERE gid = {receiver.GroupId};";
                    int i = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    if (i == 1)
                    {
                        cmd.CommandText = $"SELECT * FROM bread WHERE gid = {receiver.GroupId};";
                        MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                        await reader.ReadAsync();
                        int maxexp_formula = (int)(300 * Math.Pow(2, reader.GetInt32("factory_level") - 1));
                        Global.time_now = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                        if (Global.time_now - reader.GetInt64("last_expfull") >= 86400)
                        {
                            if (Global.time_now - reader.GetInt64("last_expgain") >= 86400)
                            {
                                using (var msc1 = new MySqlConnection(Global.connectstring))
                                {
                                    await msc1.OpenAsync();
                                    MySqlCommand cmd1 = new()
                                    {
                                        Connection = msc1
                                    };
                                    cmd1.CommandText = $"UPDATE bread SET exp_gained_today = 0, last_expgain = {Global.time_now} WHERE gid = {receiver.GroupId};";
                                    await cmd1.ExecuteNonQueryAsync();
                                    if (reader.GetInt32("exp_gained_today") <= maxexp_formula)
                                    {
                                        cmd1.CommandText = $"UPDATE bread SET factory_exp = {reader.GetInt32("factory_exp") + 1}, exp_gained_today = {reader.GetInt32("exp_gained_today") + 1} WHERE gid = {receiver.GroupId};";
                                        await cmd1.ExecuteNonQueryAsync();
                                    }
                                    else
                                    {
                                        cmd1.CommandText = $"UPDATE bread SET last_expfull = {Global.time_now}, exp_gained_today = 0 WHERE gid = {receiver.GroupId};";
                                        await cmd1.ExecuteNonQueryAsync();
                                        try
                                        {
                                            await MessageManager.SendGroupMessageAsync(receiver.GroupId, "本群已达到今日获取经验上限！");
                                        }
                                        catch
                                        {
                                            Console.WriteLine("群消息发送失败");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (reader.GetInt32("exp_gained_today") < maxexp_formula)
                                {
                                    using (var msc1 = new MySqlConnection(Global.connectstring))
                                    {
                                        await msc1.OpenAsync();
                                        MySqlCommand cmd1 = new()
                                        {
                                            Connection = msc1
                                        };
                                        cmd1.CommandText = $"UPDATE bread SET factory_exp = {reader.GetInt32("factory_exp") + 1}, exp_gained_today = {reader.GetInt32("exp_gained_today") + 1} WHERE gid = {receiver.GroupId};";
                                        await cmd1.ExecuteNonQueryAsync();
                                    }
                                }
                                else
                                {
                                    using (var msc1 = new MySqlConnection(Global.connectstring))
                                    {
                                        await msc1.OpenAsync();
                                        MySqlCommand cmd1 = new()
                                        {
                                            Connection = msc1
                                        };
                                        cmd1.CommandText = $"UPDATE bread SET last_expfull = {Global.time_now}, exp_gained_today = 0 WHERE gid = {receiver.GroupId};";
                                        await cmd1.ExecuteNonQueryAsync();
                                    }
                                    try
                                    {
                                        await MessageManager.SendGroupMessageAsync(receiver.GroupId, "本群已达到今日获取经验上限！");
                                    }
                                    catch
                                    {
                                        Console.WriteLine("群消息发送失败");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        // 升级工厂
        public static async void UpgradeFactory(string group)
        {
            // 连接数据库
            using (var msc = new MySqlConnection(Global.connectstring))
            {
                await msc.OpenAsync();
                MySqlCommand cmd = new()
                {
                    Connection = msc
                };
                // 判断数据是否存在
                cmd.CommandText = $"SELECT COUNT(*) gid FROM bread WHERE gid = {group};";
                int i = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (i == 1)
                {
                    cmd.CommandText = $"SELECT * FROM bread WHERE gid = {group};";
                    MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                    await reader.ReadAsync();
                    int exp_formula = (int)(900 * Math.Pow(2, reader.GetInt32("factory_level") - 1));
                    if (reader.GetInt32("factory_level") < breadfactory_maxlevel)
                    {
                        if (reader.GetInt32("factory_exp") >= exp_formula)
                        {
                            using (var msc1 = new MySqlConnection(Global.connectstring))
                            {
                                await msc1.OpenAsync();
                                MySqlCommand cmd1 = new()
                                {
                                    Connection = msc1
                                };
                                cmd1.CommandText = $"UPDATE bread SET factory_level = {reader.GetInt32("factory_level") + 1}, factory_exp = {reader.GetInt32("factory_exp") - exp_formula} WHERE gid = {group};";
                                await cmd1.ExecuteNonQueryAsync();
                            }
                            await reader.CloseAsync();
                            cmd.CommandText = $"SELECT * FROM bread WHERE gid = {group};";
                            reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                            await reader.ReadAsync();
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(group, $"恭喜，本群面包厂升级成功辣！现在面包厂等级是 {reader.GetInt32("factory_level")} 级");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(group, $"很抱歉，目前本群还需要 {exp_formula - reader.GetInt32("factory_exp")} 经验才能升级");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(group, "本群面包厂已经满级了！（tips：可以输入/upgrade_storage来升级库存）");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                    }
                }
                else
                {
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(group, "本群还没有面包厂！");
                    }
                    catch
                    {
                        Console.WriteLine("群消息发送失败");
                    }
                }
            }
        }
        // 升级库存
        public static async void UpgradeStorage(string group)
        {
            using (var msc = new MySqlConnection(Global.connectstring))
            {
                await msc.OpenAsync();
                MySqlCommand cmd = new()
                {
                    Connection = msc
                };
                // 判断数据是否存在
                cmd.CommandText = $"SELECT COUNT(*) gid FROM bread WHERE gid = {group};";
                int i = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (i == 1)
                {
                    cmd.CommandText = $"SELECT * FROM bread WHERE gid = {group};";
                    MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                    await reader.ReadAsync();
                    int exp_formula = (int)(2000 * Math.Pow(1.28, reader.GetInt32("storage_upgraded")));
                    if (reader.GetInt32("factory_level") == breadfactory_maxlevel)
                    {
                        if (reader.GetInt32("factory_exp") >= exp_formula)
                        {
                            if (reader.GetInt32("storage_upgraded") < 16)
                            {
                                using (var msc1 = new MySqlConnection(Global.connectstring))
                                {
                                    await msc1.OpenAsync();
                                    MySqlCommand cmd1 = new()
                                    {
                                        Connection = msc1
                                    };
                                    cmd1.CommandText = $"UPDATE bread SET storage_upgraded = {reader.GetInt32("storage_upgraded") + 1}, factory_exp = {reader.GetInt32("factory_exp") - exp_formula} WHERE gid = {group};";
                                    await cmd1.ExecuteNonQueryAsync();
                                }
                                await reader.CloseAsync();
                                cmd.CommandText = $"SELECT * FROM bread WHERE gid = {group};";
                                reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                                await reader.ReadAsync();
                                try
                                {
                                    await MessageManager.SendGroupMessageAsync(group, $"恭喜，本群面包厂库存升级成功辣！现在面包厂可以储存 {(int)(32 * Math.Pow(4, reader.GetInt32("factory_level") - 1) * Math.Pow(2, reader.GetInt32("storage_upgraded")))} 块面包");
                                }
                                catch
                                {
                                    Console.WriteLine("群消息发送失败");
                                }
                            }
                            else
                            {
                                try
                                {
                                    await MessageManager.SendGroupMessageAsync(group, "本群面包厂库存已经无法再升级了！（tips：目前本群面包厂的库存已经可以存放2^30块面包了！）");
                                }
                                catch
                                {
                                    Console.WriteLine("群消息发送失败");
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(group, $"很抱歉，目前本群还需要 {exp_formula - reader.GetInt32("factory_exp")} 经验才能升级");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(group, $"本群面包厂尚未满级！（tips：面包厂满级为 {breadfactory_maxlevel} 级）");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                    }
                }
            }
        }
        // 查询原材料库存
        public static async void QueryMaterial(string group, string executor)
        {
            // 连接数据库
            using (var msc = new MySqlConnection(Global.connectstring))
            {
                await msc.OpenAsync();
                MySqlCommand cmd = new()
                {
                    Connection = msc
                };
                try
                {
                    cmd.CommandText = $"SELECT * FROM material WHERE gid = {group};";
                    MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                    await reader.ReadAsync();
                    MessageChain? messageChain = new MessageChainBuilder()
                    .At(executor)
                    .Plain($" 现在库存有 {reader.GetInt32("flour")} 份面粉、{reader.GetInt32("egg")} 份鸡蛋、{reader.GetInt32("yeast")} 份酵母")
                    .Build();
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(group, messageChain);
                    }
                    catch
                    {
                        Console.WriteLine("群消息发送失败");
                    }
                }
                catch
                {
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(group, "本群还没有面包厂！");
                    }
                    catch
                    {
                        Console.WriteLine("群消息发送失败");
                    }
                }
            }
        }
        // 查询生产模式
        public static async void QueryMode(string group)
        {
            // 连接数据库
            using (var msc = new MySqlConnection(Global.connectstring))
            {
                await msc.OpenAsync();
                MySqlCommand cmd = new()
                {
                    Connection = msc
                };
                cmd.CommandText = $"SELECT * FROM bread WHERE gid = {group};";
                MySqlDataReader reader = (MySqlDataReader)await cmd.ExecuteReaderAsync();
                await reader.ReadAsync();
                switch (reader.GetInt32("bread_diversity"))
                {
                    case 2:
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(group, "本群供应模式为：无限供应");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                        break;
                    case 1:
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(group, "本群供应模式为：多样化供应");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                        break;
                    case 0:
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(group, "本群供应模式为：单一化供应");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                        break;
                }
            }
        }
    }
}
