// 2kbot，一款用C#编写的基于mirai和mirai.net的自由机器人软件
// Copyright(C) 2022 Abjust 版权所有。

// 本程序是自由软件：你可以根据自由软件基金会发布的GNU Affero通用公共许可证的条款，即许可证的第3版或（您选择的）任何后来的版本重新发布它和/或修改它。。

// 本程序的发布是希望它能起到作用。但没有任何保证；甚至没有隐含的保证。本程序的分发是希望它是有用的，但没有任何保证，甚至没有隐含的适销对路或适合某一特定目的的保证。 参见 GNU Affero通用公共许可证了解更多细节。

// 您应该已经收到了一份GNU Affero通用公共许可证的副本。 如果没有，请参见<https://www.gnu.org/licenses/>。

// 致所有构建及修改2kbot代码片段的用户：作者（Abjust）并不承担构建2kbot代码片段（包括修改过的版本）所产生的一切风险，但是用户有权在2kbot的GitHub项目页提出issue，并有权在代码片段修复这些问题后获取这些更新，但是，作者不会对修改过的代码版本做质量保证，也没有义务修正在修改过的代码片段中存在的任何缺陷。

using Manganese.Text;
using Mirai.Net.Data.Events.Concretes.Group;
using Mirai.Net.Data.Events.Concretes.Message;
using Mirai.Net.Data.Events.Concretes.Request;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Data.Shared;
using Mirai.Net.Sessions;
using Mirai.Net.Sessions.Http.Managers;
using Mirai.Net.Utils.Scaffolds;
using MySql.Data.MySqlClient;
using Net_2kBot.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reactive.Linq;

namespace Net_2kBot
{
    public static class BotMain
    {
        public static async Task Main()
        {
            // 初始化全局变量
            if (!System.IO.File.Exists("global.txt"))
            {
                string[] lines =
                {
                    "owner_qq=", "api=", "api_key=","bot_qq=","verify_key=","database_host=","database_user=","database_passwd=","database_name="
                };
                System.IO.File.Create("global.txt").Close();
                await System.IO.File.WriteAllLinesAsync("global.txt", lines);
                Console.WriteLine("全局变量文件已创建！现在，你需要前往项目文件夹或者程序文件夹找到global.txt并按照要求编辑");
                Environment.Exit(0);
            }
            else
            {
                foreach (string line in System.IO.File.ReadLines("global.txt"))
                {
                    string[] split = line.Split("=");
                    if (split.Length == 2)
                    {
                        switch (split[0])
                        {
                            case "owner_qq":
                                Global.owner_qq = split[1];
                                break;
                            case "api":
                                Global.api = split[1];
                                break;
                            case "api_key":
                                Global.api_key = split[1];
                                break;
                            case "bot_qq":
                                Global.bot_qq = split[1];
                                break;
                            case "verify_key":
                                Global.verify_key = split[1];
                                break;
                            case "database_host":
                                Global.database_host = split[1];
                                break;
                            case "database_user":
                                Global.database_user = split[1];
                                break;
                            case "database_passwd":
                                Global.database_passwd = split[1];
                                break;
                            case "database_name":
                                Global.database_name = split[1];
                                break;
                        }
                    }
                }
                Global.connectstring = $"server={Global.database_host};userid={Global.database_user};password={Global.database_passwd};database={Global.database_name}";
            }
            // 启动机器人程序
            MiraiBot bot = new()
            {
                Address = "localhost:8080",
                QQ = Global.bot_qq,
                VerifyKey = Global.verify_key
            };
            // 注意: `LaunchAsync`是一个异步方法，请确保`Main`方法的返回值为`Task`
            await bot.LaunchAsync();
            // 启动成功提示
            Console.WriteLine("2kbot已启动！");
            // 初始化
            // 连接数据库
            using (var msc = new MySqlConnection(Global.connectstring))
            {
                await msc.OpenAsync();
                MySqlCommand cmd = new()
                {
                    Connection = msc
                };
                // 若数据表不存在则创建
                cmd.CommandText = @$"
CREATE TABLE IF NOT EXISTS `{Global.database_name}`.`blocklist` (`id` INT NOT NULL AUTO_INCREMENT,`qid` VARCHAR(10) NOT NULL COMMENT 'QQ号',`gid` VARCHAR(10) NOT NULL COMMENT 'Q群号',PRIMARY KEY (`id`));
CREATE TABLE IF NOT EXISTS `{Global.database_name}`.`ops` (`id` INT NOT NULL AUTO_INCREMENT,`qid` VARCHAR(10) NOT NULL COMMENT 'QQ号',`gid` VARCHAR(10) NOT NULL COMMENT 'Q群号',PRIMARY KEY (`id`));
CREATE TABLE IF NOT EXISTS `{Global.database_name}`.`ignores` (`id` INT NOT NULL AUTO_INCREMENT,`qid` VARCHAR(10) NOT NULL COMMENT 'QQ号',`gid` VARCHAR(10) NOT NULL COMMENT 'Q群号',PRIMARY KEY (`id`));
CREATE TABLE IF NOT EXISTS `{Global.database_name}`.`g_blocklist` (`id` INT NOT NULL AUTO_INCREMENT,`qid` VARCHAR(10) NOT NULL COMMENT 'QQ号',PRIMARY KEY (`id`));
CREATE TABLE IF NOT EXISTS `{Global.database_name}`.`g_ops` (`id` INT NOT NULL AUTO_INCREMENT,`qid` VARCHAR(10) NOT NULL COMMENT 'QQ号',PRIMARY KEY (`id`));
CREATE TABLE IF NOT EXISTS `{Global.database_name}`.`g_ignores` (`id` INT NOT NULL AUTO_INCREMENT,`qid` VARCHAR(10) NOT NULL COMMENT 'QQ号',PRIMARY KEY (`id`));
CREATE TABLE IF NOT EXISTS `{Global.database_name}`.`repeatctrl` (`id` INT NOT NULL AUTO_INCREMENT,`qid` VARCHAR(10) NOT NULL COMMENT 'QQ号',`gid` VARCHAR(10) NOT NULL COMMENT 'Q群号',`last_repeat` bigint NOT NULL DEFAULT '946656000' COMMENT '上次复读时间',`last_repeatctrl` bigint NOT NULL DEFAULT '946656000' COMMENT '上次复读控制时间',`repeat_count` TINYINT UNSIGNED NOT NULL DEFAULT 0 COMMENT '复读计数',PRIMARY KEY (`id`));
CREATE TABLE IF NOT EXISTS `{Global.database_name}`.`bread` (
  `id` int NOT NULL AUTO_INCREMENT,
  `gid` varchar(10) NOT NULL COMMENT 'Q群号',
  `factory_level` int NOT NULL DEFAULT '1' COMMENT '面包厂等级',
  `storage_upgraded` int NOT NULL DEFAULT '0' COMMENT '库存升级次数',
  `bread_diversity` tinyint NOT NULL DEFAULT '0' COMMENT '多样化生产状态',
  `factory_exp` int NOT NULL DEFAULT '0' COMMENT '面包厂经验',
  `breads` int NOT NULL DEFAULT '0' COMMENT '面包库存',
  `exp_gained_today` int NOT NULL DEFAULT '0' COMMENT '近24小时获取经验数',
  `last_expfull` bigint NOT NULL DEFAULT '946656000' COMMENT '上次达到经验上限时间',
  `last_expgain` bigint NOT NULL DEFAULT '946656000' COMMENT '近24小时首次获取经验时间',
  `last_produce` bigint NOT NULL DEFAULT '946656000' COMMENT '上次完成一轮生产周期时间',
  PRIMARY KEY (`id`));
CREATE TABLE IF NOT EXISTS `{Global.database_name}`.`material` (
  `id` int NOT NULL AUTO_INCREMENT,
  `gid` varchar(10) NOT NULL COMMENT 'Q群号',
  `flour` int NOT NULL DEFAULT 0 COMMENT '面粉数量',
  `egg` int NOT NULL DEFAULT 0 COMMENT '鸡蛋数量',
  `yeast` int NOT NULL DEFAULT 0 COMMENT '酵母数量',
  `last_produce` bigint NOT NULL DEFAULT '946656000' COMMENT '上次完成一轮生产周期时间',
  PRIMARY KEY (`id`));
INSERT IGNORE INTO `{Global.database_name}`.`material` (id, gid) SELECT id, gid FROM `2kbot`.`bread`";
                await cmd.ExecuteNonQueryAsync();
            }
            // 在这里添加你的代码，比如订阅消息/事件之类的
            Update.Execute();
            // 戳一戳效果
            bot.EventReceived
            .OfType<NudgeEvent>()
            .Subscribe(receiver =>
            {
                if (receiver.Target == Global.bot_qq && receiver.Subject.Kind == "Group")
                {
                    Zuan.Execute(receiver.FromId, receiver.Subject.Id, @event: receiver);
                }
            });
            // bot加群
            bot.EventReceived
            .OfType<NewInvitationRequestedEvent>()
            .Subscribe(async e =>
            {
                if (e.FromId == Global.owner_qq)
                {
                    // 同意邀请
                    await RequestManager.HandleNewInvitationRequestedAsync(e, NewInvitationRequestHandlers.Approve, "");
                    Console.WriteLine("机器人已同意加入 " + e.GroupId);
                }
                else
                {
                    // 拒绝邀请
                    await RequestManager.HandleNewInvitationRequestedAsync(e, NewInvitationRequestHandlers.Reject, "");
                    Console.WriteLine("机器人已拒绝加入 " + e.GroupId);
                }
            });
            // 侦测加群请求
            bot.EventReceived
            .OfType<NewMemberRequestedEvent>()
            .Subscribe(async e =>
            {
                if ((Global.blocklist != null && Global.blocklist.Contains($"{e.GroupId}_{e.FromId}")) || (Global.blocklist != null && Global.blocklist.Contains(e.FromId)))
                {
                    await e.RejectAsync();
                }
            });
            // 侦测改名
            bot.EventReceived
            .OfType<MemberCardChangedEvent>()
            .Subscribe(async receiver =>
            {
                if (receiver.Current != "")
                {
                    try
                    {
                        await MessageManager.SendGroupMessageAsync(receiver.Member.Group.Id, $"QQ号：{receiver.Member.Id}\r\n原昵称：{receiver.Origin}\r\n新昵称：{receiver.Current}");
                    }
                    catch
                    {
                        Console.WriteLine("侦测到改名");
                    }
                }

            });
            // 侦测撤回
            bot.EventReceived
           .OfType<GroupMessageRecalledEvent>()
           .Subscribe(async receiver =>
           {
               var messageChain = new MessageChainBuilder()
                .At(receiver.Operator.Id)
                .Plain(" 你他妈撤回了什么？")
                .Build();
               if (receiver.AuthorId != receiver.Operator.Id)
               {
                   if (receiver.Operator.Permission.ToString() != "Administrator" && receiver.Operator.Permission.ToString() != "Owner")
                   {
                       try
                       {
                           await MessageManager.SendGroupMessageAsync(receiver.Group.Id, messageChain);
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
                       await MessageManager.SendGroupMessageAsync(receiver.Group.Id, messageChain);
                   }
                   catch
                   {
                       Console.WriteLine("群消息发送失败");
                   }
               }
           });
            // 侦测踢人
            bot.EventReceived
            .OfType<MemberKickedEvent>()
            .Subscribe(async receiver =>
            {
                try
                {
                    await MessageManager.SendGroupMessageAsync(receiver.Member.Group.Id, $"{receiver.Member.Name} ({receiver.Member.Id}) 被踢出去辣，好似，开香槟咯！");
                }
                catch
                {
                    Console.WriteLine("群消息发送失败");
                }
            });
            // 侦测退群
            bot.EventReceived
            .OfType<MemberLeftEvent>()
            .Subscribe(async receiver =>
            {
                try
                {
                    await MessageManager.SendGroupMessageAsync(receiver.Member.Group.Id, $"{receiver.Member.Name} ({receiver.Member.Id}) 退群力（悲）");
                }
                catch
                {
                    Console.WriteLine("群消息发送失败");
                }
            });
            // 侦测入群
            bot.EventReceived
            .OfType<MemberJoinedEvent>()
            .Subscribe(async receiver =>
            {
                MessageChain? messageChain = new MessageChainBuilder()
               .At(receiver.Member.Id)
               .Plain(" 来辣，让我们一起撅新人！（bushi")
               .Build();
                try
                {
                    await MessageManager.SendGroupMessageAsync(receiver.Member.Group.Id, messageChain);
                }
                catch
                {
                    Console.WriteLine("群消息发送失败");
                }
            });
            // bot对接收消息的处理
            bot.MessageReceived
            .OfType<GroupMessageReceiver>()
            .Subscribe(async x =>
            {
                if ((Global.ignores == null || !Global.ignores.Contains($"{x.GroupId}_{x.Sender.Id}")) && (Global.g_ignores == null || !Global.g_ignores.Contains(x.Sender.Id)))
                {
                    // 面包厂相关
                    string[] text1 = x.MessageChain.GetPlainMessage().Split(" ");
                    if (text1.Length == 2)
                    {
                        int number;
                        switch (text1[0])
                        {
                            case "给我面包":
                                if (int.TryParse(text1[1], out number))
                                {
                                    Bread.Give(x.GroupId, x.Sender.Id, number);
                                }
                                break;
                            case "给你面包":
                                if (int.TryParse(text1[1], out number))
                                {
                                    Bread.Get(x.GroupId, x.Sender.Id, number);
                                }
                                break;
                            case "更改模式":
                                switch (text1[1])
                                {
                                    case "infinite":
                                        Bread.ChangeMode(x.GroupId, 2);
                                        break;
                                    case "diversity":
                                        Bread.ChangeMode(x.GroupId, 1);
                                        break;
                                    case "normal":
                                        Bread.ChangeMode(x.GroupId, 0);
                                        break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (text1[0])
                        {
                            case "查询面包库存":
                                Bread.Query(x.GroupId, x.Sender.Id);
                                break;
                            case "升级面包厂":
                                Bread.UpgradeFactory(x.GroupId);
                                break;
                            case "建设面包厂":
                                Bread.BuildFactory(x.GroupId);
                                break;
                            case "升级库存":
                                Bread.UpgradeStorage(x.GroupId);
                                break;
                            case "查询原材料库存":
                                Bread.QueryMaterial(x.GroupId, x.Sender.Id);
                                break;
                            case "查询生产模式":
                                Bread.QueryMode(x.GroupId);
                                break;
                        }
                    }
                    // 计算经验
                    Bread.GetExp(x);
                    // 复读机
                    Repeat.Execute(x);
                    // 数学计算
                    if (x.MessageChain.GetPlainMessage().StartsWith("计算器"))
                    {
                        Mathematics.Execute(x);
                    }
                    // 发送公告
                    if (x.MessageChain.GetPlainMessage().StartsWith("发布公告"))
                    {
                        IEnumerable<Group> groups = AccountManager.GetGroupsAsync().GetAwaiter().GetResult();
                        Announce.Execute(x, x.Sender.Id, groups);
                    }
                    // surprise
                    if (x.MessageChain.GetPlainMessage() == "惊喜")
                    {
                        MessageChain? chain = new MessageChainBuilder()
                             .VoiceFromPath(Global.path + "/ysxb.slk")
                             .Build();
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(x.GroupId, chain);
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                    }
                    // 随机图片
                    if (x.MessageChain.GetPlainMessage() == "图")
                    {
                        Random r = new();
                        string url;
                        int chance = 3;
                        int choice = r.Next(chance);
                        if (choice == chance - 1)
                        {
                            url = "https://www.dmoe.cc/random.php";
                        }
                        else
                        {
                            url = "https://source.unsplash.com/random";
                        }
                        MessageChain? chain = new MessageChainBuilder()
                             .ImageFromUrl(url)
                             .Build();
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(x.GroupId, "图片在来的路上...");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(x.GroupId, chain);
                        }
                        catch
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "图片好像不见了！再等等吧？");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    // 菜单与帮助
                    Help.Execute(x);
                    // 叫人
                    if (x.MessageChain.GetPlainMessage().StartsWith("叫人"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;//正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        switch (text.Length)
                        {
                            case 3:
                                try
                                {
                                    if (text[2].ToInt32() >= 1)
                                    {
                                        Call.Execute(text[1], x.GroupId, text[2].ToInt32());
                                    }
                                    else if (text[2].ToInt32() < 1)
                                    {
                                        try
                                        {
                                            await MessageManager.SendGroupMessageAsync(x.GroupId, "nmd，这个数字是几个意思？");
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
                                        await MessageManager.SendGroupMessageAsync(x.GroupId, "油饼食不食？");
                                    }
                                    catch
                                    {
                                        Console.WriteLine("群消息发送失败");
                                    }
                                }
                                break;
                            case 2:
                                try
                                {
                                    if (ja.Count == 4)
                                    {
                                        string target = ja[2]["target"]!.ToString();
                                        string t = ja[3]["text"]!.ToString().Replace(" ", "");
                                        int time = t.ToInt32();
                                        try
                                        {
                                            if (time >= 1)
                                            {
                                                Call.Execute(target, x.GroupId, time);
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    await MessageManager.SendGroupMessageAsync(x.GroupId, "nmd，这个数字是几个意思？");
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
                                                await MessageManager.SendGroupMessageAsync(x.GroupId, "油饼食不食？");
                                            }
                                            catch
                                            {
                                                Console.WriteLine("群消息发送失败");
                                            }
                                        }
                                    }
                                    else if (ja.Count == 3)
                                    {
                                        string target = ja[2]["target"]!.ToString();
                                        Call.Execute(target, x.GroupId, 3);
                                    }
                                    else
                                    {
                                        Call.Execute(text[1], x.GroupId, 3);
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("群消息发送失败");
                                }
                                break;
                            case < 2:
                                try
                                {
                                    await MessageManager.SendGroupMessageAsync(x.GroupId, "缺少参数");
                                }
                                catch
                                {
                                    Console.WriteLine("群消息发送失败");
                                }
                                break;
                            default:
                                try
                                {
                                    await MessageManager.SendGroupMessageAsync(x.GroupId, "缺少参数");
                                }
                                catch
                                {
                                    Console.WriteLine("群消息发送失败");
                                }
                                break;
                        }
                    }
                    // 鸣谢
                    if (x.MessageChain.GetPlainMessage() == "鸣谢")
                    {
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(x.GroupId,
                            "特别感谢Windows 2000的代码支持");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                    }
                     // 处理“你就是歌姬吧”（祖安）
                    Zuan.Execute(x.Sender.Id, x.GroupId, x.MessageChain);
                    // 群管功能
                    // 禁言
                    if (x.MessageChain.GetPlainMessage().StartsWith("/mute") && !x.MessageChain.GetPlainMessage().StartsWith("禁言"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;  //正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length != 1)
                        {
                            switch (text.Length)
                            {
                                case 3:
                                    Admin.Mute(x.Sender.Id, text[1], x.GroupId, x.Sender.Permission.ToString(), text[2].ToInt32());
                                    break;
                                case 2:
                                    switch (ja.Count)
                                    {
                                        case 4:
                                            string t = ja[3]["text"]!.ToString().Replace(" ", "");
                                            string target = ja[2]["target"]!.ToString();
                                            if (t == "")
                                            {
                                                Admin.Mute(x.Sender.Id, target, x.GroupId, x.Sender.Permission.ToString(), 10);
                                            }
                                            else
                                            {
                                                int time = t.ToInt32();
                                                Admin.Mute(x.Sender.Id, target, x.GroupId, x.Sender.Permission.ToString(), time);
                                            }
                                            break;
                                        case 3:
                                            string target1 = ja[2]["target"]!.ToString();
                                            Admin.Mute(x.Sender.Id, target1, x.GroupId, x.Sender.Permission.ToString(), 10);
                                            break;
                                        case 2:
                                            Admin.Mute(x.Sender.Id, text[1], x.GroupId, x.Sender.Permission.ToString(), 10);
                                            break;
                                    }
                                    break;
                                default:
                                    {
                                        try
                                        {
                                            await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                                        }
                                        catch
                                        {
                                            Console.WriteLine("群消息发送失败");
                                        }
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    // 解禁
                    if (x.MessageChain.GetPlainMessage().StartsWith("解禁"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;  //正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.Unmute(x.Sender.Id, target, x.GroupId, x.Sender.Permission.ToString());
                                    break;
                                case 2:
                                    Admin.Unmute(x.Sender.Id, text[1], x.GroupId, x.Sender.Permission.ToString());
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    // 踢人
                    if (x.MessageChain.GetPlainMessage().StartsWith("踢人"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;  //正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.Kick(x.Sender.Id, target, x.GroupId, x.Sender.Permission.ToString());
                                    break;
                                case 2:
                                    Admin.Kick(x.Sender.Id, text[1], x.GroupId, x.Sender.Permission.ToString());
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    // 加黑
                    if (x.MessageChain.GetPlainMessage().StartsWith("添加黑名单"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;  //正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.Block(x.Sender.Id, target, x.GroupId, x.Sender.Permission.ToString());
                                    break;
                                case 2:
                                    Admin.Block(x.Sender.Id, text[1], x.GroupId, x.Sender.Permission.ToString());
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch { }
                        }
                    }
                    // 解黑
                    if (x.MessageChain.GetPlainMessage().StartsWith("解除黑名单"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;  //正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.Unblock(x.Sender.Id, target, x.GroupId, x.Sender.Permission.ToString());
                                    break;
                                case 2:
                                    Admin.Unblock(x.Sender.Id, text[1], x.GroupId, x.Sender.Permission.ToString());
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    // 全局加黑
                    if (x.MessageChain.GetPlainMessage().StartsWith("添加全局黑名单"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;  //正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.G_Block(x.Sender.Id, target, x.GroupId);
                                    break;
                                case 2:
                                    Admin.G_Block(x.Sender.Id, text[1], x.GroupId);
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch { }
                        }
                    }
                    // 全局解黑
                    if (x.MessageChain.GetPlainMessage().StartsWith("解除全局黑名单"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;  //正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.G_Unblock(x.Sender.Id, target, x.GroupId);
                                    break;
                                case 2:
                                    Admin.G_Unblock(x.Sender.Id, text[1], x.GroupId);
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    // 给予机器人管理员
                    if (x.MessageChain.GetPlainMessage().StartsWith("给机器人管理员"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;//正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.Op(x.Sender.Id, target, x.GroupId, x.Sender.Permission.ToString());
                                    break;
                                case 2:
                                    Admin.Op(x.Sender.Id, text[1], x.GroupId, x.Sender.Permission.ToString());
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch { }
                        }
                    }
                    // 剥夺机器人管理员
                    if (x.MessageChain.GetPlainMessage().StartsWith("取消机器人管理员"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;  //正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        IEnumerable<Member> members = new List<Member>();
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.Deop(x.Sender.Id, target, x.GroupId);
                                    break;
                                case 2:
                                    Admin.Deop(x.Sender.Id, text[1], x.GroupId);
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    // 给予全局机器人管理员
                    if (x.MessageChain.GetPlainMessage().StartsWith("给全局机器人管理员"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;//正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.G_Op(x.Sender.Id, target, x.GroupId);
                                    break;
                                case 2:
                                    Admin.G_Op(x.Sender.Id, text[1], x.GroupId);
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch { }
                        }
                    }
                    // 剥夺全局机器人管理员
                    if (x.MessageChain.GetPlainMessage().StartsWith("取消机器人全局黑名单"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;  //正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.G_Deop(x.Sender.Id, target, x.GroupId);
                                    break;
                                case 2:
                                    Admin.G_Deop(x.Sender.Id, text[1], x.GroupId);
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch
                            {
                                Console.WriteLine("群消息发送失败");
                            }
                        }
                    }
                    // 屏蔽消息
                    if (x.MessageChain.GetPlainMessage().StartsWith("屏蔽消息"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;//正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.Ignore(x.Sender.Id, target, x.GroupId, x.Sender.Permission.ToString());
                                    break;
                                case 2:
                                    Admin.Ignore(x.Sender.Id, text[1], x.GroupId, x.Sender.Permission.ToString());
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch { }
                        }
                    }
                    // 全局屏蔽消息
                    if (x.MessageChain.GetPlainMessage().StartsWith("全局屏蔽消息"))
                    {
                        string result1 = x.MessageChain.ToJsonString();
                        JArray ja = (JArray)JsonConvert.DeserializeObject(result1)!;//正常获取jobject
                        string[] text = ja[1]["text"]!.ToString().Split(" ");
                        if (text.Length == 2)
                        {
                            switch (ja.Count)
                            {
                                case 3:
                                    string target = ja[2]["target"]!.ToString();
                                    Admin.G_Ignore(x.Sender.Id, target, x.GroupId);
                                    break;
                                case 2:
                                    Admin.G_Ignore(x.Sender.Id, text[1], x.GroupId);
                                    break;
                            }
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch { }
                        }
                    }
                    // 禁言自己
                    if (x.MessageChain.GetPlainMessage().StartsWith("禁言自己"))
                    {
                        string[] text = x.MessageChain.GetPlainMessage().Split(" ");
                        if (text.Length == 2)
                        {
                            Admin.MuteMe(x.Sender.Id, x.GroupId, text[1].ToInt32());
                        }
                        else if (text.Length == 1)
                        {
                            Admin.MuteMe(x.Sender.Id, x.GroupId, 10);
                        }
                        else
                        {
                            try
                            {
                                await MessageManager.SendGroupMessageAsync(x.GroupId, "参数错误");
                            }
                            catch { }
                        }
                    }
                    // 发动带清洗
                    if (x.MessageChain.GetPlainMessage() == ("大清洗"))
                    {
                        Admin.Purge(x.Sender.Id, x.GroupId, x.Sender.Permission.ToString());
                    }
                    // 版本
                    if (x.MessageChain.GetPlainMessage() == "版本")
                    {
                        List<string> splashes = new()
                    {
                        "也试试HanBot罢！Also try HanBot!",
                        "誓死捍卫微软苏维埃！",
                        "打倒MF独裁分子！",
                        "要把反革命分子的恶臭思想，扫进历史的垃圾堆！",
                        "PHP是世界上最好的编程语言（雾）",
                        "社会主义好，社会主义好~",
                        "Minecraft很好玩，但也可以试试Terraria！",
                        "So Nvidia, f**k you!",
                        "战无不胜的马克思列宁主义万岁！",
                        "Bug是杀不完的，你杀死了一个Bug，就会有千千万万个Bug站起来！",
                        "跟张浩扬博士一起来学Jvav罢！",
                        "哼哼哼，啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊",
                        "你知道吗？其实你什么都不知道！",
                        "Tips:这是一条烫...烫..烫知识（）",
                        "你知道成功的秘诀吗？我告诉你成功的秘诀就是：我操你妈的大臭逼",
                        "有时候ctmd不一定是骂人 可能是传统美德",
                        "python不一定是编程语言 也可能是屁眼通红",
                        "这条标语虽然没有用，但是是有用的，因为他被加上了标语",
                        "使用C#编写！"
                    };
                        Random r = new();
                        int random = r.Next(splashes.Count);
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(x.GroupId,
                            $"机器人版本：alpha 0.0.1\r\n上次更新日期：2022/12/14\r\n更新内容：机器人的首个版本\r\n---------\r\n{splashes[random]}");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                    }
                    // 获取源码
                    if (x.MessageChain.GetPlainMessage() == "源码" || (x.MessageChain.GetPlainMessage() == "获取源码") || (x.MessageChain.GetPlainMessage() == "怎样做这样的机器人"))
                    {
                        try
                        {
                            await MessageManager.SendGroupMessageAsync(x.GroupId, "请前往https://github.com/123Windows31/2kbot获取3.1bot的源码！");
                        }
                        catch
                        {
                            Console.WriteLine("群消息发送失败");
                        }
                    }
                }
            });
            // 运行面包厂生产任务
            var Tasks = new Task[]
            {
                Task.Run(async () => await BreadFactory.MaterialProduce()),
                Task.Run(async () => await BreadFactory.BreadProduce())
            };
            await Task.WhenAll(Tasks);
            Console.ReadLine();
        }
    }
}