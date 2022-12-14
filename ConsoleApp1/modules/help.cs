// 2kbot，一款用C#编写的基于mirai和mirai.net的自由机器人软件
// Copyright(C) 2022 Abjust 版权所有。

// 本程序是自由软件：你可以根据自由软件基金会发布的GNU Affero通用公共许可证的条款，即许可证的第3版或（您选择的）任何后来的版本重新发布它和/或修改它。。

// 本程序的发布是希望它能起到作用。但没有任何保证；甚至没有隐含的保证。本程序的分发是希望它是有用的，但没有任何保证，甚至没有隐含的适销对路或适合某一特定目的的保证。 参见 GNU Affero通用公共许可证了解更多细节。

// 您应该已经收到了一份GNU Affero通用公共许可证的副本。 如果没有，请参见<https://www.gnu.org/licenses/>。

// 致所有构建及修改2kbot代码片段的用户：作者（Abjust）并不承担构建2kbot代码片段（包括修改过的版本）所产生的一切风险，但是用户有权在2kbot的GitHub项目页提出issue，并有权在代码片段修复这些问题后获取这些更新，但是，作者不会对修改过的代码版本做质量保证，也没有义务修正在修改过的代码片段中存在的任何缺陷。

using Manganese.Text;
using Mirai.Net.Data.Messages;
using Mirai.Net.Data.Messages.Receivers;
using Mirai.Net.Utils.Scaffolds;

namespace Net_2kBot.Modules
{
    public static class Help
    {
        public static async void Execute(MessageReceiverBase @base)
        {
            if (@base is GroupMessageReceiver receiver)
            {
                // 菜单
                if (receiver.MessageChain.GetPlainMessage() == "菜单" || receiver.MessageChain.GetPlainMessage() == "菜单")
                {
                    try
                    {
                        await receiver.SendMessageAsync(@"3.1bot菜单
1.群管系统
2.复读机
3.叫人功能
4.精神心理疾病科普
5.量表测试
6.面包厂功能
7.获取源码
8.数学计算器
详情请用 帮助 指令");
                    }
                    catch
                    {
                        Console.WriteLine("菜单消息发送失败");
                    }
                }
                // 帮助
                var indexs = new List<string>
                {
                    "1",
                    "2",
                    "3",
                    "4",
                    "5",
                    "6",
                    "7",
                    "8"
                };
                var contents = new List<string>
                {
                    @"群管功能
禁言：禁言 <QQ号或at> [时间] （以分钟算）
解禁：解禁 <QQ号或at>
踢出：踢出 <QQ号或at>
加黑：添加黑名单 <QQ号或at>
解黑：解除黑名单 <QQ号或at>
屏蔽消息（加灰）：屏蔽消息 <QQ号或at>
给予管理员：给机器人管理员 <QQ号或at>
剥夺管理员：取消机器人管理员 <QQ号或at>
（上述功能都需要机器人管理员）",
                    "该指令用于复述文本\r\n用法：复读 <文本>",
                    "该指令用于叫人\r\n用法：叫人 <QQ号或at> [次数]",
                    @"面包厂功能
建造面包厂（初始化）：建设面包厂
给3.1kbot面包： 给面包 <数量>
向3.1bot要面包：要面包 <数量>
查询面包库存：查询面包库存
查询原材料库存：查询原材料库存
查询生产（供应）模式：查询生产模式
修改生产（供应）模式：更改模式 <infinite/diversity/normal> （无限、多样化、单一化）
升级面包厂：升级面包厂
升级库存（满级后）：升级库存",
                    "https://github.com/123Windows31/2kbot",
                    "使用计算器可以显示计算器说明"
                };
                if (receiver.MessageChain.GetPlainMessage().StartsWith("帮助") == true)
                {
                    string[] result = receiver.MessageChain.GetPlainMessage().Split(" ");
                    if (result.Length == 2)
                    {
                        foreach (string q in indexs)
                        {
                            try
                            {
                                if (result[1] == q)
                                {
                                    try
                                    {
                                        await receiver.SendMessageAsync((contents[indexs.IndexOf(q)]));
                                    }
                                    catch
                                    {
                                        Console.WriteLine("帮助消息发送失败");
                                    }
                                }
                                else if (result[1].ToInt32() > indexs.Count)
                                {
                                    try
                                    {
                                        await receiver.SendMessageAsync("未找到相关帮助");
                                    }
                                    catch
                                    {
                                        Console.WriteLine("帮助消息发送失败");
                                    }
                                    break;
                                }
                            }
                            catch
                            {
                                try
                                {
                                    await receiver.SendMessageAsync("请写数字，不要写别的好吗？");
                                }
                                catch
                                {
                                    Console.WriteLine("帮助消息发送失败");
                                }
                                break;
                            }
                        }
                    }
                    else if (receiver.MessageChain.GetPlainMessage() == "帮助")
                    {
                        try
                        {
                            await receiver.SendMessageAsync(@"目前有对于以下功能的帮助文档：
[1]群管功能
[2]复读
[3]叫人
[4]面包厂
[5]获取源码
[6]数学计算器");
                        }
                        catch
                        {
                            Console.WriteLine("帮助消息发送失败");
                        }
                    }
                }
            }
        }
    }
}