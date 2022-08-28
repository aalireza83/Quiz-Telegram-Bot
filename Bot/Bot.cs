using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot
{
    public partial class Bot : Form
    {
        Repository.IUserRepository UserRepository;
        Repository.ISettingsRepository SettingsRepository;
        Repository.IDeleteRepository DeleteRepository;
        Repository.IDownloadRepository DownloadRepository;
        Repository.IMessagesRepository MessagesRepository;
        Repository.IPayRepository PayRepository;
        Repository.ITransactionsRepository TransactionsRepository;
        Repository.IQuestionsRepository QuestionsRepository;
        Repository.IExamsRepository ExamsRepository;
        Repository.IExamAccessRepository ExamAccessRepository;
        private static string Token = "1669764835:AAE_m0WtuLtyVLtmmNVoARHn4cZ44LyY3PM";
        long CahtId_Admin = 107574787;
        string ChatId_Channel = "-1001354639008";
        private Thread botThread;
        private Telegram.Bot.TelegramBotClient bot;
        public static bool stopbot = false;
        bool closefrom = false, pused = false, AutoBackUp = true, QuestionByQuestion = true, AnswerSheet = true;
        string Fileid = "";
        int lefttime = 7200, MainTime = 7200;
        Keyboards Keyboards = new Keyboards();
        DateTime removeTime;
        public Bot()
        {
            InitializeComponent();
            UserRepository = new Repository.UserRepository();
            SettingsRepository = new Repository.SettingsRepository();
            DeleteRepository = new Repository.DeleteRepository();
            DownloadRepository = new Repository.DownloadRepository();
            MessagesRepository = new Repository.MessagesRepository();
            PayRepository = new Repository.PayRepository();
            TransactionsRepository = new Repository.TransactionsRepository();
            QuestionsRepository = new Repository.QuestionsRepository();
            ExamsRepository = new Repository.ExamsRepository();
            ExamAccessRepository = new Repository.ExamAccessRepository();
        }
        private void Bot_Load(object sender, EventArgs e)
        {
            SettingsRepository.SelectSettings();
            try
            {
                this.Hide();
                this.ShowInTaskbar = false;
                //DataTable dt = SettingsRepository.SelectSettings();
                //stopbot = (bool)dt.Rows[0][1];
                //pused = (bool)dt.Rows[0][2];
                //QuestionByQuestion = (bool)dt.Rows[0][3];
                //AnswerSheet = (bool)dt.Rows[0][4];
                //Fileid = dt.Rows[0][5].ToString();
                //AutoBackUp = (bool)dt.Rows[0][6];
                //foreach (DataRow row in DeleteRepository.SelectDate().Rows)
                //    removeTime = Convert.ToDateTime(row[0]);
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                botThread = new Thread(new ThreadStart(runBot));
                botThread.Start();
            }
            catch { }
        }
        void runBot()
        {
            bot = new Telegram.Bot.TelegramBotClient(Token);
            int offset = 0;
            while (true)
            {
                try
                {
                    Telegram.Bot.Types.Update[] update = bot.GetUpdatesAsync(offset).Result;
                    if (closefrom)
                        FormHandler_Formclose();
                    foreach (var up in update)
                    {
                        offset = up.Id + 1;
                        var lastcommand = "";
                        long chatId;
                        var text = "";
                        string signuped;
                        bool named = false;
                        DataTable Userdata;
                        if (up.CallbackQuery != null)
                        {
                            chatId = up.CallbackQuery.Message.Chat.Id;
                            Userdata = UserRepository.SelectRow(chatId);
                            lastcommand = Userdata.Rows[0][5].ToString();
                            signuped = Userdata.Rows[0][2].ToString();
                            if (chatId == CahtId_Admin && signuped.StartsWith("Management-"))
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine("لطفا پیام خود را ارسال کنید");
                                bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, up.CallbackQuery.Message.MessageId, Keyboards.BackKeyboardMarkup);
                                adder("Reply-" + up.CallbackQuery.Data + "-" + up.CallbackQuery.Message.MessageId, chatId);
                            }
                            else if (up.CallbackQuery.Data == "Buy")
                            {
                                bot.SendTextMessageAsync(chatId, "مبلغ مورد نظر خود را به تومان ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                adder("Buy", chatId);
                            }
                        }
                        if (up.Message == null)
                            continue;
                        if (up.Message.Text != null)
                            text = up.Message.Text.ToLower();
                        chatId = up.Message.Chat.Id;
                        if (!UserRepository.UserExists(chatId))
                        {
                            UserRepository.Insert(chatId, "", "", DateTime.Now, "", "/start", 0);
                            bot.SendTextMessageAsync(107574787, "[" + chatId + "](tg://user?id=" + chatId + ") ربات را استارت کرد" + " \U00002705", ParseMode.MarkdownV2);
                            bot.SendTextMessageAsync(-1001354639008, "[" + chatId + "](tg://user?id=" + chatId + ") ربات را استارت کرد" + " \U00002705", ParseMode.MarkdownV2);
                            if (TransactionsRepository.Insert(10000, chatId, 0, "هدیه از طرف ادمین", DateTime.Now, "اعتبار اولیه"))
                            {
                                if (UserRepository.UpdateAmount(chatId, 10000))
                                    bot.SendTextMessageAsync(chatId, "مبلغ 10000 تومان هدیه ادمین به عنوان اعتبار اولیه به حساب شما اضافه شد");
                                else
                                    bot.SendTextMessageAsync(CahtId_Admin, "خطا در تغییر موجودی");
                            }
                            else
                                bot.SendTextMessageAsync(chatId, "خطا در ثبت تراکنش", ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.ManagementKeyboard);
                        }
                        Userdata = UserRepository.SelectRow(chatId);
                        lastcommand = Userdata.Rows[0][5].ToString();
                        if (Userdata.Rows[0][1].ToString() != "")
                            named = true;
                        signuped = Userdata.Rows[0][2].ToString();
                        string phone = Userdata.Rows[0][4].ToString();
                        if (stopbot && chatId != CahtId_Admin)
                            bot.SendTextMessageAsync(chatId, "ربات توسط ادمین متوقف شده است" + " \U0001F6AB");
                        else if (text == "مدیریت" && chatId == CahtId_Admin && !signuped.StartsWith("Management-"))
                        {
                            UserRepository.UpdatePanel(chatId, "Management-" + signuped);
                            bot.SendTextMessageAsync(chatId, "به پنل مدیریت خوش آمدید", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                            adder("/start", chatId);
                        }
                        else if (signuped.StartsWith("Management-"))
                        {
                            if (text == "/start")
                            {
                                bot.SendTextMessageAsync(chatId, "به پنل مدیریت خوش آمدید", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                            else if (text == "صغحه اصلی" + " \U0001F3E0")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }

                            #region مدیریت کاربران
                            else if (text == "مدیریت کاربران" + " \U0001F465" && lastcommand == "/start")
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "مدیریت کاربران" + " \U0001F465")
                            {
                                if (UserRepository.UserExists(long.Parse(text)))
                                {
                                    StringBuilder stringBuilder = new StringBuilder();
                                    DataTable User = UserRepository.SelectRow(long.Parse(text));
                                    stringBuilder.AppendLine("چت آیدی : [" + User.Rows[0][0] + "](tg://user?id=" + User.Rows[0][0] + ")" + Environment.NewLine + "نام : " + User.Rows[0][1]);
                                    if (User.Rows[0][2].ToString() == "")
                                    {
                                        stringBuilder.AppendLine("وضعیت ثبت نام : " + "تایید نشده");
                                    }
                                    if (User.Rows[0][2].ToString() == "Verified")
                                    {
                                        stringBuilder.AppendLine("وضعیت ثبت نام : " + "تایید شده");
                                    }
                                    if (User.Rows[0][2].ToString() == "Blocked")
                                    {
                                        stringBuilder.AppendLine("وضعیت ثبت نام : " + "بلاک شده");
                                    }
                                    if (User.Rows[0][2].ToString().StartsWith("Management"))
                                    {
                                        stringBuilder.AppendLine("وضعیت ثبت نام : " + "مدیریت");
                                    }
                                    stringBuilder.AppendLine("تاریخ ثبت نام : " + User.Rows[0][3]);
                                    stringBuilder.AppendLine("شماره تلفن : " + User.Rows[0][4]);
                                    stringBuilder.AppendLine("موجودی : " + User.Rows[0][6] + " تومان");
                                    bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                    ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                    KeyboardMarkup.ResizeKeyboard = true;
                                    KeyboardButton[] row1 =
                                    {
                                  new KeyboardButton("حذف کاربر" + " \U0001F5D1"), new KeyboardButton("تغییر وضعیت" + " \U0001F504"),new KeyboardButton("تغییر نام" + " \U0001F58A")
                            };
                                    KeyboardButton[] row2 =
                                    {
                                  new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
                            };
                                    KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                        {
                            row1,row2
                                        };
                                    bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                    adder("chatid-" + text, chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "چت آیدی وارد شده اشتباه میباشد", ParseMode.Default, false, false, 0);
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "مدیریت کاربران" + " \U0001F465")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                            else if (text == "حذف کاربر" + " \U0001F5D1" && lastcommand.StartsWith("chatid-"))
                            {
                                UserRepository.Delete(long.Parse(lastcommand.Replace("chatid-", string.Empty)));
                                bot.SendTextMessageAsync(chatId, "کاربر حذف شد " + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                bot.SendTextMessageAsync(ChatId_Channel, "کاربر " + "[" + lastcommand.Replace("chatid-", string.Empty) + "](tg://user?id=" + lastcommand.Replace("chatid-", string.Empty) + ") حذف شد" + " \U00002705", ParseMode.MarkdownV2);
                                adder("/start", chatId);
                            }
                            #region تغییر وضعیت
                            else if (text == "تغییر وضعیت" + " \U0001F504" && lastcommand.StartsWith("chatid-"))
                            {
                                ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                KeyboardMarkup.ResizeKeyboard = true;
                                KeyboardButton[] row1 =
                                {
                                  new KeyboardButton("تغییر وضعیت به بلاک شده" + " \U000026D4"), new KeyboardButton("تغییر وضعیت به تایید شده" + " \U00002705"), new KeyboardButton("تغییر وضعیت به تایید نشده" + " \U00002754")
                            };
                                KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                    {
                            row1
                                    };
                                KeyboardButton[] row2 =
                                {
                                  new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
                            };
                                KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                    {
                            row1,row2
                                    };
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                adder("ChangeStatus-" + lastcommand.Split('-')[1], chatId);
                            }
                            else if (text == "تغییر وضعیت به بلاک شده" + " \U000026D4" && lastcommand.StartsWith("ChangeStatus-"))
                            {
                                UserRepository.UpdatePanel(long.Parse(lastcommand.Split('-')[1]), "Blocked");
                                bot.SendTextMessageAsync(chatId, "بلاک انجام شد" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                bot.SendTextMessageAsync(lastcommand.Split('-')[1], "متاسفانه شما توسط ادمین بلاک شدید" + " \U000026D4", ParseMode.Default, false, false, 0, Keyboards.BlockedKeyboardMarkup);
                                bot.SendTextMessageAsync(ChatId_Channel, "[" + lastcommand.Split('-')[1] + "](tg://user?id=" + lastcommand.Split('-')[1] + ") بلاک شد" + " \U00002705", ParseMode.MarkdownV2);
                                adder("/start", chatId);
                            }
                            else if (text == "تغییر وضعیت به تایید شده" + " \U00002705" && lastcommand.StartsWith("ChangeStatus-"))
                            {
                                UserRepository.UpdatePanel(long.Parse(lastcommand.Split('-')[1]), "Verified");
                                bot.SendTextMessageAsync(chatId, "تایید انجام شد" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                bot.SendTextMessageAsync(lastcommand.Split('-')[1], "حساب کاربری شما توسط ادمین تایید شد" + " \U0001F513", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                bot.SendTextMessageAsync(ChatId_Channel, "[" + lastcommand.Split('-')[1] + "](tg://user?id=" + lastcommand.Split('-')[1] + ") تایید شد" + " \U00002705", ParseMode.MarkdownV2);
                                adder("/start", chatId);
                            }
                            else if (text == "تغییر وضعیت به تایید نشده" + " \U00002754" && lastcommand.StartsWith("ChangeStatus-"))
                            {
                                UserRepository.UpdatePanel(long.Parse(lastcommand.Split('-')[1]), "");
                                DataTable data = UserRepository.SelectRow(long.Parse(lastcommand.Split('-')[1]));
                                ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                KeyboardMarkup.ResizeKeyboard = true;
                                if (data.Rows[0][1].ToString() == "")
                                {
                                    KeyboardMarkup = Keyboards.SignupKeyboardMarkup;
                                }
                                else if (data.Rows[0][4].ToString() == "")
                                {
                                    KeyboardMarkup = Keyboards.signupedKeyboardMarkup;
                                }
                                else
                                {
                                    KeyboardMarkup = Keyboards.finishKeyboardMarkup;
                                }
                                bot.SendTextMessageAsync(lastcommand.Split('-')[1], "حساب کاربری شما توسط ادمین به حالت تایید نشده تغییر پیدا کرد", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                bot.SendTextMessageAsync(chatId, "تغییر وضعیت به تایید نشده انجام شد" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                bot.SendTextMessageAsync(ChatId_Channel, "تغییر وضعیت [" + lastcommand.Split('-')[1] + "](tg://user?id=" + lastcommand.Split('-')[1] + ") به تایید نشده انجام شد" + " \U00002705", ParseMode.MarkdownV2);
                                adder("/start", chatId);
                            }
                            #endregion
                            #region تغییر نام
                            else if (text == "تغییر نام" + " \U0001F58A" && lastcommand.StartsWith("chatid-"))
                            {
                                ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                KeyboardMarkup.ResizeKeyboard = true;
                                KeyboardButton[] row1 =
                                {
                                  new KeyboardButton("حذف نام" + " \U0001F5D1"),
                            };
                                KeyboardButton[] row2 =
                                {
                                  new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
                            };
                                KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                    {
                            row1,row2
                                    };
                                bot.SendTextMessageAsync(chatId, "اسم مورد نظر را برای ویرایش ارسال کنید", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                adder("ChangeName-" + lastcommand.Split('-')[1], chatId);
                            }
                            else if (text == "حذف نام" + " \U0001F5D1" && lastcommand.StartsWith("ChangeName-"))
                            {
                                UserRepository.ChangeName(long.Parse(lastcommand.Split('-')[1]), "");
                                bot.SendTextMessageAsync(chatId, "حذف نام انجام شد" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                bot.SendTextMessageAsync(ChatId_Channel, "حذف نام برای [" + lastcommand.Split('-')[1] + "](tg://user?id=" + lastcommand.Split('-')[1] + ") انجام شد" + " \U00002705", ParseMode.MarkdownV2);
                                adder("/start", chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand.StartsWith("ChangeName-"))
                            {
                                UserRepository.ChangeName(long.Parse(lastcommand.Split('-')[1]), text);
                                bot.SendTextMessageAsync(chatId, "تغییر نام انجام شد" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                bot.SendTextMessageAsync(ChatId_Channel, "تغییر نام کاربر [" + lastcommand.Split('-')[1] + "](tg://user?id=" + lastcommand.Split('-')[1] + ") به نام " + text + " انجام شد" + " \U00002705", ParseMode.MarkdownV2);
                                adder("/start", chatId);
                            }
                            #endregion
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand.StartsWith("chatid-"))
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                adder("مدیریت کاربران" + " \U0001F465", chatId);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand.StartsWith("Change"))
                            {
                                if (UserRepository.UserExists(long.Parse(lastcommand.Split('-')[1])))
                                {
                                    StringBuilder stringBuilder = new StringBuilder();
                                    DataTable User = UserRepository.SelectRow(long.Parse(lastcommand.Replace("chatid-", string.Empty)));
                                    stringBuilder.AppendLine("چت آیدی : [" + User.Rows[0][0] + "](tg://user?id=" + User.Rows[0][0] + ")" + Environment.NewLine + "نام : " + User.Rows[0][1]);
                                    if (User.Rows[0][2].ToString() == "")
                                    {
                                        stringBuilder.AppendLine("وضعیت ثبت نام : " + "تایید نشده");
                                    }
                                    if (User.Rows[0][2].ToString() == "Verified")
                                    {
                                        stringBuilder.AppendLine("وضعیت ثبت نام : " + "تایید شده");
                                    }
                                    if (User.Rows[0][2].ToString() == "Blocked")
                                    {
                                        stringBuilder.AppendLine("وضعیت ثبت نام : " + "بلاک شده");
                                    }
                                    if (User.Rows[0][2].ToString().StartsWith("Management"))
                                    {
                                        stringBuilder.AppendLine("وضعیت ثبت نام : " + "مدیریت");
                                    }
                                    stringBuilder.AppendLine("تاریخ ثبت نام : " + User.Rows[0][3]);
                                    stringBuilder.AppendLine("شماره تلفن : " + User.Rows[0][4]);
                                    stringBuilder.AppendLine("موجودی : " + User.Rows[0][6] + " تومان");
                                    bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                    ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                    KeyboardMarkup.ResizeKeyboard = true;
                                    KeyboardButton[] row1 =
                                    {
                                  new KeyboardButton("حذف کاربر" + " \U0001F5D1"), new KeyboardButton("تغییر وضعیت" + " \U0001F504"),new KeyboardButton("تغییر نام" + " \U0001F58A")
                            };
                                    KeyboardButton[] row2 =
                                    {
                                  new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
                            };
                                    KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                        {
                            row1,row2
                                        };
                                    bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                    adder("مدیریت کاربران" + " \U0001F465", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "چت آیدی وارد شده اشتباه میباشد", ParseMode.Default, false, false, 0);
                                }
                            }
                            #endregion

                            #region مدیریت آزمون ها
                            else if (text == "مدیریت آزمون ها" + " \U0001F4DA" && lastcommand == "/start")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد آزمون : " + row[0]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[1]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F4DA لیست 10 آزمون اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "کد آزمون مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.ExamManagement);
                                adder(text, chatId);
                            }
                            else if (text == "همه آزمون ها" + " \U0001F4DA" && lastcommand == "مدیریت آزمون ها" + " \U0001F4DA")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد آزمون : " + row[0]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[1]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    File.WriteAllText(Environment.CurrentDirectory + "\\Report.txt", stringBuilder.ToString());
                                    FileStream fs = System.IO.File.OpenRead(Environment.CurrentDirectory + "\\Report.txt");
                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, "همه آزمون ها.txt");
                                    bot.SendDocumentAsync(chatId, inputOnlineFile);
                                    File.Delete(Environment.CurrentDirectory + "\\Report.txt");
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                                bot.SendTextMessageAsync(chatId, "کد آزمون مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.ExamManagement);
                            }
                            else if (text == "آپدیت" + " \U0001F504" && lastcommand == "مدیریت آزمون ها" + " \U0001F4DA")
                            {
                                new Thread(delegate ()
                                {
                                    UpdateExams(up, chatId);
                                }).Start();
                                removeTime = default;
                                foreach (DataRow row in DeleteRepository.SelectDate().Rows)
                                    removeTime = Convert.ToDateTime(row[0]);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "مدیریت آزمون ها" + " \U0001F4DA")
                            {
                                if (ExamsRepository.ExamExist(long.Parse(text)))
                                {
                                    StringBuilder stringBuilder = new StringBuilder();
                                    foreach (DataRow row in ExamsRepository.SelectRow(long.Parse(text)).Rows)
                                    {
                                        stringBuilder.AppendLine("کد آزمون : " + row[0]);
                                        stringBuilder.AppendLine("نام آزمون : " + row[1]);
                                        stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[2]);
                                        stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[3]);
                                        stringBuilder.AppendLine("تعداد سوال : " + row[4] + Environment.NewLine + Environment.NewLine);
                                    }
                                    bot.SendTextMessageAsync(chatId, stringBuilder.ToString());
                                    bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.ExamEditor);
                                    adder("Exam-" + text, chatId);
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "مدیریت آزمون ها" + " \U0001F4DA")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                            #region ارسال آزمون
                            else if (text == "ارسال آزمون" + " \U0001F4E4" && lastcommand.StartsWith("Exam-"))
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید یا یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.KeyboardUsersType);
                                adder(lastcommand.Replace("Exam-", "Send-"), chatId);
                            }
                            else if (text == "همه کاربران" + " \U0001F465" && lastcommand.StartsWith("Send-"))
                            {
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.KeyboardSend);
                                adder(lastcommand.Replace("Send-", "SendExam-") + "-All", chatId);
                            }
                            else if (text == "کاربران تایید شده" + " \U00002705" && lastcommand.StartsWith("Send-"))
                            {
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.KeyboardSend);
                                adder(lastcommand.Replace("Send-", "SendExam-") + "-Verified", chatId);
                            }
                            else if (text == "کاربران بلاک شده" + " \U000026D4" && lastcommand.StartsWith("Send-"))
                            {
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.KeyboardSend);
                                adder(lastcommand.Replace("Send-", "SendExam-") + "-Blocked", chatId);
                            }
                            else if (text == "کاربران تایید نشده" + " \U00002754" && lastcommand.StartsWith("Send-"))
                            {
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.KeyboardSend);
                                adder(lastcommand.Replace("Send-", "SendExam-") + "-Nothing", chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand.StartsWith("Send-"))
                            {
                                if (UserRepository.UserExists(long.Parse(text)))
                                {
                                    bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.KeyboardSend);
                                    adder(lastcommand.Replace("Send-", "SendExam-") + "-" + text, chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "چت آیدی وارد شده اشتباه میباشد", ParseMode.Default, false, false, 0);
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand.StartsWith("Send-"))
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamsRepository.SelectRow(long.Parse(text)).Rows)
                                {
                                    stringBuilder.AppendLine("کد آزمون : " + row[0]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[1]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4] + Environment.NewLine + Environment.NewLine);
                                }
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.ExamEditor);
                                adder(lastcommand.Replace("Send-", "Exam-"), chatId);
                            }
                            else if (text == "ارسال پاسخنامه متنی" + " \U0001F4CB" && lastcommand.StartsWith("SendExam-"))
                            {
                                new Thread(delegate ()
                                {
                                    SendAS(lastcommand.Split('-')[1], lastcommand.Split('-')[2], chatId);
                                }).Start();
                            }
                            else if (text == "ارسال سوال به سوال" + " \U0001F5BC" && lastcommand.StartsWith("SendExam-"))
                            {
                                new Thread(delegate ()
                                {
                                    sendQBQ(lastcommand.Split('-')[1], lastcommand.Split('-')[2], chatId);
                                }).Start();
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand.StartsWith("SendExam-"))
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید یا یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.KeyboardUsersType);
                                adder("Send-" + lastcommand.Split('-')[1], chatId);
                            }
                            #endregion
                            #region افزودن دسترسی
                            else if (text == "افزودن دسترسی" + " \U0001F3F7" && lastcommand.StartsWith("Exam-"))
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید یا یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.KeyboardUsersType);
                                adder(lastcommand.Replace("Exam-", "Assign-"), chatId);
                            }
                            else if (text == "همه کاربران" + " \U0001F465" && lastcommand.StartsWith("Assign-"))
                            {
                                new Thread(delegate ()
                                {
                                    AssignExam(lastcommand, chatId, "All");
                                }).Start();
                            }
                            else if (text == "کاربران تایید شده" + " \U00002705" && lastcommand.StartsWith("Assign-"))
                            {
                                new Thread(delegate ()
                                {
                                    AssignExam(lastcommand, chatId, "Verified");
                                }).Start();
                            }
                            else if (text == "کاربران بلاک شده" + " \U000026D4" && lastcommand.StartsWith("Assign-"))
                            {
                                new Thread(delegate ()
                                {
                                    AssignExam(lastcommand, chatId, "Blocked");
                                }).Start();
                            }
                            else if (text == "کاربران تایید نشده" + " \U00002754" && lastcommand.StartsWith("Assign-"))
                            {
                                new Thread(delegate ()
                                {
                                    AssignExam(lastcommand, chatId, "");
                                }).Start();
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand.StartsWith("Assign-"))
                            {
                                new Thread(delegate ()
                                {
                                    AssignExam(lastcommand, chatId, text);
                                }).Start();
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand.StartsWith("Assign-"))
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد آزمون : " + row[0]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[1]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F4DA لیست 10 آزمون اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "کد آزمون مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.ExamManagement);
                                adder(lastcommand.Replace("Assign-", "Exam-"), chatId);
                            }
                            #endregion
                            #region تعیین زمان پایان
                            else if (text == "تعیین زمان پایان" + " \U000023F2" && lastcommand.StartsWith("Exam-"))
                            {
                                bot.SendTextMessageAsync(chatId, "تاریخ و ساعت مورد نظر خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder(lastcommand.Replace("Exam-", "Time-"), chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand.StartsWith("Time-"))
                            {
                                ExamsRepository.UpdateTime(long.Parse(lastcommand.Split('-')[1]), DateTime.Parse(text));
                                bot.SendTextMessageAsync(chatId, "زمان تعیین شد", ParseMode.Default, false, false, 0, Keyboards.ExamEditor);
                                adder(lastcommand.Replace("Time", "Exam"), chatId);
                                removeTime = default;
                                foreach (DataRow row in DeleteRepository.SelectDate().Rows)
                                    removeTime = Convert.ToDateTime(row[0]);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand.StartsWith("Time-"))
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamsRepository.SelectRow(long.Parse(lastcommand.Split('-')[1].ToString())).Rows)
                                {
                                    stringBuilder.AppendLine("کد آزمون : " + row[0]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[1]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4] + Environment.NewLine + Environment.NewLine);
                                }
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.ExamEditor);
                                adder(lastcommand.Replace("Time", "Exam"), chatId);
                            }
                            #endregion
                            else if (text == "دریافت سوالات" + " \U0001F4E5" && lastcommand.StartsWith("Exam-"))
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in QuestionsRepository.SelectByExamCode(long.Parse(lastcommand.Split('-')[1])).Rows)
                                {
                                    stringBuilder.AppendLine("شماره سوال : " + row[2]);
                                    stringBuilder.AppendLine("سوال : " + row[3]);
                                    stringBuilder.AppendLine("جواب : " + row[4]);
                                    stringBuilder.AppendLine("کلید : " + row[5]);
                                    stringBuilder.AppendLine("درجه سختی : " + row[6] + Environment.NewLine + Environment.NewLine);
                                }
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.Default, true);
                            }
                            else if (text == "حذف آزمون" + " \U0001F5D1" && lastcommand.StartsWith("Exam-"))
                            {
                                if (ExamsRepository.Delete(long.Parse(lastcommand.Split('-')[0])))
                                {
                                    bot.SendTextMessageAsync(chatId, "آزمون حذف شد", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                    adder("/start", chatId);
                                }
                                removeTime = default;
                                foreach (DataRow row in DeleteRepository.SelectDate().Rows)
                                    removeTime = Convert.ToDateTime(row[0]);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand.StartsWith("Exam-"))
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد آزمون : " + row[0]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[1]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F4DA لیست 10 آزمون اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "کد آزمون مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.ExamManagement);
                                adder("مدیریت آزمون ها" + " \U0001F4DA", chatId);
                            }
                            #endregion

                            #region مدیریت دسترسی ها
                            else if (text == "مدیریت دسترسی ها" + " \U0001F3F7" && lastcommand == "/start")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamAccessRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("چت آیدی : " + row[0]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[1]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[4]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[5] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F4DA لیست 10 دسترسی اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.AccessManagement);
                                adder(text, chatId);
                            }
                            else if (text == "حذف دسترسی" + " \U0001F5D1" && lastcommand == "مدیریت دسترسی ها" + " \U0001F3F7")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine("اطلاعات را به صورت زیر ارسال کنید");
                                stringBuilder.AppendLine("چت آیدی");
                                stringBuilder.AppendLine("کد آزمون");
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "حذف دسترسی" + " \U0001F5D1")
                            {
                                StringReader sr = new StringReader(text);
                                if (ExamAccessRepository.Delete(long.Parse(sr.ReadLine()), long.Parse(sr.ReadLine())))
                                    bot.SendTextMessageAsync(chatId, "حذف دسترسی انجام شد", ParseMode.Default, false, false, 0, Keyboards.AccessManagement);
                                else
                                    bot.SendTextMessageAsync(chatId, "حذف دسترسی انجام نشد", ParseMode.Default, false, false, 0, Keyboards.AccessManagement);
                                adder("مدیریت دسترسی ها" + " \U0001F3F7", chatId);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "حذف دسترسی" + " \U0001F5D1")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamAccessRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("چت آیدی : " + row[0]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[1]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[4]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[5] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F4DA لیست 10 دسترسی اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.AccessManagement);
                                adder("مدیریت دسترسی ها" + " \U0001F3F7", chatId);
                            }
                            else if (text == "جست و جو بر اساس آزمون" + " \U0001F4DA" && lastcommand == "مدیریت دسترسی ها" + " \U0001F3F7")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد آزمون : " + row[0]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[1]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F4DA لیست آزمون ها : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "کد آزمون مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.ExamManagement);
                                adder("AccessExamSearch", chatId);
                            }
                            else if (text != "بازگشت" && lastcommand == "AccessExamSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamAccessRepository.SelectByExam(long.Parse(text)).Rows)
                                {
                                    stringBuilder.AppendLine("چت آیدی : " + row[0]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[1]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[4]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[5] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    bot.SendTextMessageAsync(chatId, stringBuilder.ToString());
                                    bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                                    adder("مدیریت دسترسی ها" + " \U0001F3F7", chatId);
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                            }
                            else if (text == "بازگشت" && lastcommand == "AccessExamSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamAccessRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("چت آیدی : " + row[0]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[1]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[4]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[5] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F4DA لیست 10 دسترسی اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.AccessManagement);
                                adder("مدیریت دسترسی ها" + " \U0001F3F7", chatId);
                            }
                            else if (text == "همه دسترسی ها" + " \U0001F4E5" && lastcommand == "مدیریت دسترسی ها" + " \U0001F3F7")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamAccessRepository.SelectAll().Rows)
                                {
                                    stringBuilder.AppendLine("چت آیدی : " + row[0]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[1]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[4]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[5] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    File.WriteAllText(Environment.CurrentDirectory + "\\Report.txt", stringBuilder.ToString());
                                    FileStream fs = System.IO.File.OpenRead(Environment.CurrentDirectory + "\\Report.txt");
                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, "همه دسترسی ها.txt");
                                    bot.SendDocumentAsync(chatId, inputOnlineFile);
                                    File.Delete(Environment.CurrentDirectory + "\\Report.txt");
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.AccessManagement);
                            }
                            else if (text == "جست و جو بر اساس چت آیدی" + " \U0001F50D" && lastcommand == "مدیریت دسترسی ها" + " \U0001F3F7")
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder("AccessUserSearch", chatId);
                            }
                            else if (text != "بازگشت" && lastcommand == "AccessUserSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamAccessRepository.SelectByChatId(long.Parse(text)).Rows)
                                {
                                    stringBuilder.AppendLine("چت آیدی : " + row[0]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[1]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[4]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[5] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    bot.SendTextMessageAsync(chatId, stringBuilder.ToString());
                                    bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                                    adder("مدیریت دسترسی ها" + " \U0001F3F7", chatId);
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                            }
                            else if (text == "بازگشت" && lastcommand == "AccessUserSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamAccessRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("چت آیدی : " + row[0]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[1]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[4]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[5] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F4DA لیست 10 دسترسی اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.AccessManagement);
                                adder("مدیریت دسترسی ها" + " \U0001F3F7", chatId);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "مدیریت دسترسی ها" + " \U0001F3F7")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                            #endregion

                            #region مدیریت دانلود ها
                            else if (text == "مدیریت دانلود ها" + " \U0001F4E5" && lastcommand == "/start")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in DownloadRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[2]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4]);
                                    stringBuilder.AppendLine("روش ارسال : " + row[5]);
                                    stringBuilder.AppendLine("ارسال توسط : " + row[6]);
                                    stringBuilder.AppendLine("نوع ارسال : " + row[7]);
                                    stringBuilder.AppendLine("تاریخ : " + row[8]);
                                    stringBuilder.AppendLine("هزینه : " + row[9] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F5C2 لیست 10 دانلود سوالات اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                                adder(text, chatId);
                            }
                            else if (text == "همه دانلود ها" + " \U0001F4E5" && lastcommand == "مدیریت دانلود ها" + " \U0001F4E5")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in DownloadRepository.SelectAll().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[2]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4]);
                                    stringBuilder.AppendLine("روش ارسال : " + row[5]);
                                    stringBuilder.AppendLine("ارسال توسط : " + row[6]);
                                    stringBuilder.AppendLine("نوع ارسال : " + row[7]);
                                    stringBuilder.AppendLine("تاریخ : " + row[8]);
                                    stringBuilder.AppendLine("هزینه : " + row[9] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    File.WriteAllText(Environment.CurrentDirectory + "\\Report.txt", stringBuilder.ToString());
                                    FileStream fs = System.IO.File.OpenRead(Environment.CurrentDirectory + "\\Report.txt");
                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, "همه دانلود ها.txt");
                                    bot.SendDocumentAsync(chatId, inputOnlineFile);
                                    File.Delete(Environment.CurrentDirectory + "\\Report.txt");
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                            }
                            #region تنظیمات دانلود سوالات
                            else if (text == "تنظیمات دانلود سوالات" + " \U00002699" && lastcommand == "مدیریت دانلود ها" + " \U0001F4E5")
                            {
                                bot.SendTextMessageAsync(chatId, StatusUpdate());
                                ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                KeyboardMarkup.ResizeKeyboard = true;
                                KeyboardButton[] row1 =
                                {
                                  new KeyboardButton("تغییر وضعیت تصویر به تصویر"+ " \U0001F5BC"),new KeyboardButton("تغییر وضعیت پاسخنامه متنی" + " \U0001F4C4")
                            };
                                KeyboardButton[] row2 =
                                {
                                  new KeyboardButton("بازگشت" + " \U0001F519")
                            };
                                KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                    {
                            row1,row2
                                    };
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text == "تغییر وضعیت تصویر به تصویر" + " \U0001F5BC" && lastcommand == "تنظیمات دانلود سوالات" + " \U00002699")
                            {
                                if (QuestionByQuestion)
                                    QuestionByQuestion = false;
                                else
                                    QuestionByQuestion = true;
                                SettingsRepository.UpdateSettings(stopbot, pused, QuestionByQuestion, AnswerSheet, Fileid, AutoBackUp);
                                bot.SendTextMessageAsync(chatId, StatusUpdate());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید");
                            }
                            else if (text == "تغییر وضعیت پاسخنامه متنی" + " \U0001F4C4" && lastcommand == "تنظیمات دانلود سوالات" + " \U00002699")
                            {
                                if (AnswerSheet)
                                    AnswerSheet = false;
                                else
                                    AnswerSheet = true;
                                SettingsRepository.UpdateSettings(stopbot, pused, QuestionByQuestion, AnswerSheet, Fileid, AutoBackUp);
                                bot.SendTextMessageAsync(chatId, StatusUpdate());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید");
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "تنظیمات دانلود سوالات" + " \U00002699")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in DownloadRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[2]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4]);
                                    stringBuilder.AppendLine("روش ارسال : " + row[5]);
                                    stringBuilder.AppendLine("ارسال توسط : " + row[6]);
                                    stringBuilder.AppendLine("نوع ارسال : " + row[7]);
                                    stringBuilder.AppendLine("تاریخ : " + row[8]);
                                    stringBuilder.AppendLine("هزینه : " + row[9] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F5C2 لیست 10 دانلود سوالات اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                                adder("مدیریت دانلود ها" + " \U0001F4E5", chatId);
                            }
                            #endregion
                            #region حذف سوالات
                            else if (text == "حذف سوالات" + " \U0001F5D1" && lastcommand == "مدیریت دانلود ها" + " \U0001F4E5")
                            {
                                ReplyKeyboardMarkup DeleteKeyboard = new ReplyKeyboardMarkup();
                                DeleteKeyboard.ResizeKeyboard = true;
                                if (!pused)
                                {
                                    KeyboardButton[] Questionmanagementrow1 =
                                {
                                         new KeyboardButton("حذف همه سوالات" + " \U0001F5D1"),new KeyboardButton("متوقف کردن حذف خودکار" + " \U000023F9")
                                };
                                    KeyboardButton[] Questionmanagementrow2 =
                                    {
                                         new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
                                };
                                    DeleteKeyboard.Keyboard = new KeyboardButton[][]
                                    {
                                       Questionmanagementrow1,Questionmanagementrow2
                                    };
                                }
                                else
                                {
                                    KeyboardButton[] Questionmanagementrow1 =
                                                                    {
                                         new KeyboardButton("حذف همه سوالات" + " \U0001F5D1"),new KeyboardButton("فعال کردن حذف خودکار" + " \U000025B6")
                                };
                                    KeyboardButton[] Questionmanagementrow2 =
                                    {
                                         new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
                                };
                                    DeleteKeyboard.Keyboard = new KeyboardButton[][]
                                    {
                                       Questionmanagementrow1,Questionmanagementrow2
                                    };
                                }
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, DeleteKeyboard);
                                adder(text, chatId);
                            }
                            else if (text == "متوقف کردن حذف خودکار" + " \U000023F9" && lastcommand == "حذف سوالات" + " \U0001F5D1" && !pused)
                            {
                                pused = true;
                                SettingsRepository.UpdateSettings(stopbot, pused, QuestionByQuestion, AnswerSheet, Fileid, AutoBackUp);
                                ReplyKeyboardMarkup DeleteKeyboard = new ReplyKeyboardMarkup();
                                DeleteKeyboard.ResizeKeyboard = true;
                                KeyboardButton[] Questionmanagementrow1 =
                                                                {
                                         new KeyboardButton("حذف همه سوالات" + " \U0001F5D1"),new KeyboardButton("فعال کردن حذف خودکار" + " \U000025B6")
                                };
                                KeyboardButton[] Questionmanagementrow2 =
                                {
                                         new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
                                };
                                DeleteKeyboard.Keyboard = new KeyboardButton[][]
                                {
                                       Questionmanagementrow1,Questionmanagementrow2
                                };
                                bot.SendTextMessageAsync(chatId, "تایمر غیر فعال شد" + " \U00002705", ParseMode.Default, false, false, 0, DeleteKeyboard);
                                bot.SendTextMessageAsync(ChatId_Channel, "تایمر غیر فعال شد" + " \U00002705");
                            }
                            else if (text == "فعال کردن حذف خودکار" + " \U000025B6" && lastcommand == "حذف سوالات" + " \U0001F5D1" && pused)
                            {
                                pused = false;
                                SettingsRepository.UpdateSettings(stopbot, pused, QuestionByQuestion, AnswerSheet, Fileid, AutoBackUp);
                                ReplyKeyboardMarkup DeleteKeyboard = new ReplyKeyboardMarkup();
                                DeleteKeyboard.ResizeKeyboard = true;
                                KeyboardButton[] Questionmanagementrow1 =
                                {
                                         new KeyboardButton("حذف همه سوالات" + " \U0001F5D1"),new KeyboardButton("متوقف کردن حذف خودکار" + " \U000023F9")
                                };
                                KeyboardButton[] Questionmanagementrow2 =
                                {
                                         new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
                                };
                                DeleteKeyboard.Keyboard = new KeyboardButton[][]
                                {
                                       Questionmanagementrow1,Questionmanagementrow2
                                };
                                bot.SendTextMessageAsync(chatId, "تایمر فعال شد" + " \U00002705", ParseMode.Default, false, false, 0, DeleteKeyboard);
                                bot.SendTextMessageAsync(ChatId_Channel, "تایمر فعال شد" + " \U00002705");
                            }
                            else if (text == "حذف همه سوالات" + " \U0001F5D1" && lastcommand == "حذف سوالات" + " \U0001F5D1")
                            {
                                if (DeleteRepository.DeleteExists())
                                {
                                    foreach (DataRow row in DeleteRepository.SelectAll().Rows)
                                    {
                                        messagedelete(long.Parse(row[1].ToString()), int.Parse(row[2].ToString()));
                                    }
                                    DeleteRepository.RemoveAll();
                                    bot.SendTextMessageAsync(chatId, "سوالات با موفقیت حذف شد" + " \U00002705");
                                    bot.SendTextMessageAsync(ChatId_Channel, "سوالات حذف شد" + " \U00002705");
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "سوالی برای حذف پیدا نشد" + " \U000026A0");
                                removeTime = default;
                                foreach (DataRow row in DeleteRepository.SelectDate().Rows)
                                    removeTime = Convert.ToDateTime(row[0]);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "حذف سوالات" + " \U0001F5D1")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in DownloadRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[2]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4]);
                                    stringBuilder.AppendLine("روش ارسال : " + row[5]);
                                    stringBuilder.AppendLine("ارسال توسط : " + row[6]);
                                    stringBuilder.AppendLine("نوع ارسال : " + row[7]);
                                    stringBuilder.AppendLine("تاریخ : " + row[8]);
                                    stringBuilder.AppendLine("هزینه : " + row[9] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F5C2 لیست 10 دانلود سوالات اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                                adder("مدیریت دانلود ها" + " \U0001F4E5", chatId);
                            }
                            #endregion
                            #region جست و جو بر اساس چت آیدی
                            else if (text == "جست و جو بر اساس چت آیدی" + " \U0001F50D" && lastcommand == "مدیریت دانلود ها" + " \U0001F4E5")
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder("QuestionSearch", chatId);
                            }
                            else if (text != "بازگشت" && lastcommand == "QuestionSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in DownloadRepository.SelectByChatId(long.Parse(text)).Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[2]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4]);
                                    stringBuilder.AppendLine("روش ارسال : " + row[5]);
                                    stringBuilder.AppendLine("ارسال توسط : " + row[6]);
                                    stringBuilder.AppendLine("نوع ارسال : " + row[7]);
                                    stringBuilder.AppendLine("تاریخ : " + row[8]);
                                    stringBuilder.AppendLine("هزینه : " + row[9] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    bot.SendTextMessageAsync(chatId, stringBuilder.ToString());
                                    bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                                    adder("مدیریت دانلود ها" + " \U0001F4E5", chatId);
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                            }
                            else if (text == "بازگشت" && lastcommand == "QuestionSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in DownloadRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[2]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4]);
                                    stringBuilder.AppendLine("روش ارسال : " + row[5]);
                                    stringBuilder.AppendLine("ارسال توسط : " + row[6]);
                                    stringBuilder.AppendLine("نوع ارسال : " + row[7]);
                                    stringBuilder.AppendLine("تاریخ : " + row[8]);
                                    stringBuilder.AppendLine("هزینه : " + row[9] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F5C2 لیست 10 دانلود سوالات اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                                adder("مدیریت دانلود ها" + " \U0001F4E5", chatId);
                            }
                            #endregion
                            #region جست و جو بر اساس آزمون
                            else if (text == "جست و جو بر اساس آزمون" + " \U0001F4DA" && lastcommand == "مدیریت دانلود ها" + " \U0001F4E5")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد آزمون : " + row[0]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[1]);
                                    stringBuilder.AppendLine("تاریخ اضافه شدن آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F4DA لیست آزمون ها : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "کد آزمون مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.ExamManagement);
                                adder("QuestionExamSearch", chatId);
                            }
                            else if (text != "بازگشت" && lastcommand == "QuestionExamSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in DownloadRepository.SelectByExam(long.Parse(text)).Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[2]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4]);
                                    stringBuilder.AppendLine("روش ارسال : " + row[5]);
                                    stringBuilder.AppendLine("ارسال توسط : " + row[6]);
                                    stringBuilder.AppendLine("نوع ارسال : " + row[7]);
                                    stringBuilder.AppendLine("تاریخ : " + row[8]);
                                    stringBuilder.AppendLine("هزینه : " + row[9] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    bot.SendTextMessageAsync(chatId, stringBuilder.ToString());
                                    bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                                    adder("مدیریت دانلود ها" + " \U0001F4E5", chatId);
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                            }
                            else if (text == "بازگشت" && lastcommand == "QuestionExamSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in DownloadRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[2]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4]);
                                    stringBuilder.AppendLine("روش ارسال : " + row[5]);
                                    stringBuilder.AppendLine("ارسال توسط : " + row[6]);
                                    stringBuilder.AppendLine("نوع ارسال : " + row[7]);
                                    stringBuilder.AppendLine("تاریخ : " + row[8]);
                                    stringBuilder.AppendLine("هزینه : " + row[9] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F5C2 لیست 10 دانلود سوالات اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                                adder("مدیریت دانلود ها" + " \U0001F4E5", chatId);
                            }
                            #endregion
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "مدیریت دانلود ها" + " \U0001F4E5")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                            #endregion

                            #region مدیریت پیام ها
                            else if (text == "مدیریت پیام ها" + " \U00002709" && lastcommand == "/start")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in MessagesRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی فرستنده : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی گیرنده : " + row[2]);
                                    stringBuilder.AppendLine("پیام : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("نوع پیام : " + row[7] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U00002709 لیست 10 پیام اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.MessagesManagement);
                                adder(text, chatId);
                            }
                            #region ارسال پیام
                            else if (text == "ارسال پیام" + "\U0001F4DD" && lastcommand == "مدیریت پیام ها" + " \U00002709")
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید یا یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.KeyboardUsersType);
                                adder(text, chatId);
                            }
                            else if (text == "همه کاربران" + " \U0001F465" && lastcommand == "ارسال پیام" + "\U0001F4DD")
                            {
                                bot.SendTextMessageAsync(chatId, "لطفا پیام خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder("SendMessage-All", chatId);
                            }
                            else if (text == "کاربران تایید شده" + " \U00002705" && lastcommand == "ارسال پیام" + "\U0001F4DD")
                            {
                                bot.SendTextMessageAsync(chatId, "لطفا پیام خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder("SendMessage-Verified", chatId);
                            }
                            else if (text == "کاربران بلاک شده" + " \U000026D4" && lastcommand == "ارسال پیام" + "\U0001F4DD")
                            {
                                bot.SendTextMessageAsync(chatId, "لطفا پیام خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder("SendMessage-Blocked", chatId);
                            }
                            else if (text == "کاربران تایید نشده" + " \U00002754" && lastcommand == "ارسال پیام" + "\U0001F4DD")
                            {
                                bot.SendTextMessageAsync(chatId, "لطفا پیام خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder("SendMessage-Nothing", chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "ارسال پیام" + "\U0001F4DD")
                            {
                                if (UserRepository.UserExists(long.Parse(text)))
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا پیام خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                    adder("SendMessage-" + text, chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "چت آیدی وارد شده اشتباه میباشد", ParseMode.Default, false, false, 0);
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "ارسال پیام" + "\U0001F4DD")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in MessagesRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی فرستنده : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی گیرنده : " + row[2]);
                                    stringBuilder.AppendLine("پیام : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("نوع پیام : " + row[7] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U00002709 لیست 10 پیام اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.MessagesManagement);
                                adder("مدیریت پیام ها" + " \U00002709", chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand.StartsWith("SendMessage-"))
                            {
                                if (up.Message.Type == MessageType.Text)
                                {
                                    if (lastcommand.Split('-')[1] == "All")
                                    {
                                        StringBuilder stringBuilder = new StringBuilder();
                                        foreach (DataRow row in UserRepository.SelectAll().Rows)
                                        {
                                            try
                                            {
                                                var message = bot.SendTextMessageAsync(row[0].ToString(), "\U0001F4E9 پیام جدید از طرف ادمین :" + Environment.NewLine + text);
                                                MessagesRepository.Insert(chatId, long.Parse(row[0].ToString()), message.Result.MessageId, text, DateTime.Now, 0, "همه کاربران");
                                                stringBuilder.AppendLine("[" + row[0].ToString() + "](tg://user?id=" + row[0].ToString() + ")" + Environment.NewLine);
                                            }
                                            catch { }
                                        }
                                        var messCh = bot.SendTextMessageAsync(ChatId_Channel, text, ParseMode.Default).Result;
                                        bot.SendTextMessageAsync(chatId, "پیام به همه کاربران با آیدی های زیر ارسال شد" + " \U00002705" + Environment.NewLine + stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.ManagementKeyboard);
                                        bot.SendTextMessageAsync(ChatId_Channel, "پیام به همه کاربران با آیدی های زیر ارسال شد" + " \U00002705" + Environment.NewLine + stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, messCh.MessageId);
                                    }
                                    else if (lastcommand.Split('-')[1] == "Verified")
                                    {
                                        DataTable Users = UserRepository.SelectByPanel("Verified");
                                        StringBuilder stringBuilder = new StringBuilder();
                                        foreach (DataRow row in Users.Rows)
                                        {
                                            try
                                            {
                                                var message = bot.SendTextMessageAsync(row[0].ToString(), "\U0001F4E9 پیام جدید از طرف ادمین :" + Environment.NewLine + text);
                                                MessagesRepository.Insert(chatId, long.Parse(row[0].ToString()), message.Result.MessageId, text, DateTime.Now, 0, "کاربران تایید شده");
                                                stringBuilder.AppendLine("[" + row[0].ToString() + "](tg://user?id=" + row[0].ToString() + ")" + Environment.NewLine);
                                            }
                                            catch { }
                                        }
                                        var messCh = bot.SendTextMessageAsync(ChatId_Channel, text, ParseMode.Default).Result;
                                        bot.SendTextMessageAsync(chatId, "پیام به کاربران تایید شده با آیدی های زیر ارسال شد" + " \U00002705" + Environment.NewLine + stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.ManagementKeyboard);
                                        bot.SendTextMessageAsync(ChatId_Channel, "پیام به کاربران تایید شده با آیدی های زیر ارسال شد" + " \U00002705" + Environment.NewLine + stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, messCh.MessageId);
                                    }
                                    else if (lastcommand.Split('-')[1] == "Blocked")
                                    {
                                        DataTable Users = UserRepository.SelectByPanel("Blocked");
                                        StringBuilder stringBuilder = new StringBuilder();
                                        foreach (DataRow row in Users.Rows)
                                        {
                                            try
                                            {
                                                var message = bot.SendTextMessageAsync(row[0].ToString(), "\U0001F4E9 پیام جدید از طرف ادمین :" + Environment.NewLine + text);
                                                MessagesRepository.Insert(chatId, long.Parse(row[0].ToString()), message.Result.MessageId, text, DateTime.Now, 0, "کاربران بلاک شده");
                                                stringBuilder.AppendLine("[" + row[0].ToString() + "](tg://user?id=" + row[0].ToString() + ")" + Environment.NewLine);
                                            }
                                            catch { }
                                        }
                                        var messCh = bot.SendTextMessageAsync(ChatId_Channel, text, ParseMode.Default).Result;
                                        bot.SendTextMessageAsync(chatId, "پیام به کاربران بلاک شده با آیدی های زیر ارسال شد" + " \U00002705" + Environment.NewLine + stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.ManagementKeyboard);
                                        bot.SendTextMessageAsync(ChatId_Channel, "پیام به به کاربران بلاک شده با آیدی های زیر ارسال شد" + " \U00002705" + Environment.NewLine + stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, messCh.MessageId);
                                    }
                                    else if (lastcommand.Split('-')[1] == "Nothing")
                                    {
                                        DataTable Users = UserRepository.SelectByPanel("");
                                        StringBuilder stringBuilder = new StringBuilder();
                                        foreach (DataRow row in Users.Rows)
                                        {
                                            try
                                            {
                                                var message = bot.SendTextMessageAsync(row[0].ToString(), "\U0001F4E9 پیام جدید از طرف ادمین :" + Environment.NewLine + text);
                                                MessagesRepository.Insert(chatId, long.Parse(row[0].ToString()), message.Result.MessageId, text, DateTime.Now, 0, "ارسال شده به همه کاربران");
                                                stringBuilder.AppendLine("[" + row[0].ToString() + "](tg://user?id=" + row[0].ToString() + ")" + Environment.NewLine);
                                            }
                                            catch { }
                                        }
                                        var messCh = bot.SendTextMessageAsync(ChatId_Channel, text, ParseMode.Default).Result;
                                        bot.SendTextMessageAsync(chatId, "پیام به کاربران تایید نشده با آیدی های زیر ارسال شد" + " \U00002705" + Environment.NewLine + stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.ManagementKeyboard);
                                        bot.SendTextMessageAsync(ChatId_Channel, "پیام به کاربران تایید نشده با آیدی های زیر ارسال شد" + " \U00002705" + Environment.NewLine + stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, messCh.MessageId);
                                    }
                                    else
                                    {
                                        var messCh = bot.SendTextMessageAsync(ChatId_Channel, text, ParseMode.Default).Result;
                                        var message = bot.SendTextMessageAsync(lastcommand.Split('-')[1], "\U0001F4E9 پیام جدید از طرف ادمین :" + Environment.NewLine + text, ParseMode.Default).Result;
                                        MessagesRepository.Insert(chatId, long.Parse(lastcommand.Replace("SendMessage-", string.Empty)), message.MessageId, text, DateTime.Now, 0, "شخصی");
                                        bot.SendTextMessageAsync(chatId, "پیام به کاربر [" + lastcommand.Replace("SendMessage-", string.Empty) + "](tg://user?id=" + lastcommand.Replace("sendmes-", string.Empty) + ") ارسال شد" + " \U00002705", ParseMode.MarkdownV2, false, false, 0, Keyboards.ManagementKeyboard);
                                        bot.SendTextMessageAsync(ChatId_Channel, "پیام به کاربر [" + lastcommand.Replace("SendMessage-", string.Empty) + "](tg://user?id=" + lastcommand.Replace("sendmes-", string.Empty) + ") ارسال شد" + " \U00002705", ParseMode.MarkdownV2, false, false, messCh.MessageId);
                                    }
                                    adder("/start", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا فقط پیام متنی ارسال کنید");
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand.StartsWith("SendMessage-"))
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید یا یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.KeyboardUsersType);
                                adder(text, chatId);
                            }
                            #endregion
                            #region جست و جو بر اساس چت آیدی
                            else if (text == "جست و جو بر اساس چت آیدی" + " \U0001F50D" && lastcommand == "مدیریت پیام ها" + " \U00002709")
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder("MessagesSearch", chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "MessagesSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in MessagesRepository.SelectByChatId(long.Parse(text)).Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی فرستنده : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی گیرنده : " + row[2]);
                                    stringBuilder.AppendLine("پیام : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("نوع پیام : " + row[7] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                    adder("/start", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "MessagesSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in MessagesRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی فرستنده : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی گیرنده : " + row[2]);
                                    stringBuilder.AppendLine("پیام : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("نوع پیام : " + row[7] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U00002709 لیست 10 پیام اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.MessagesManagement);
                                adder("مدیریت پیام ها" + " \U00002709", chatId);
                            }
                            #endregion
                            else if (text == "همه پیام ها" + " \U00002709" && lastcommand == "مدیریت پیام ها" + " \U00002709")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in MessagesRepository.SelectAll().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی فرستنده : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی گیرنده : " + row[2]);
                                    stringBuilder.AppendLine("پیام : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("نوع پیام : " + row[7] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    File.WriteAllText(Environment.CurrentDirectory + "\\Report.txt", stringBuilder.ToString());
                                    FileStream fs = System.IO.File.OpenRead(Environment.CurrentDirectory + "\\Report.txt");
                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, "همه پیام ها.txt");
                                    bot.SendDocumentAsync(chatId, inputOnlineFile);
                                    File.Delete(Environment.CurrentDirectory + "\\Report.txt");
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.MessagesManagement);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "مدیریت پیام ها" + " \U00002709")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                            #endregion

                            #region تنظیمات ربات
                            else if (text == "تنظیمات ربات" + " \U00002699" && lastcommand == "/start")
                            {
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.BotSettings);
                                adder(text, chatId);
                            }
                            else if (text == "بکاپ" + " \U0001F4BE" && lastcommand == "تنظیمات ربات" + " \U00002699")
                            {
                                try
                                {
                                    BackUp();
                                    bot.SendTextMessageAsync(chatId, "بک آپ گیری انجام شد" + " \U00002705");
                                    bot.SendTextMessageAsync(ChatId_Channel, "بک آپ گیری انجام شد" + " \U00002705");
                                }
                                catch
                                {
                                    bot.SendTextMessageAsync(chatId, "خطا در بک آپ گیری" + " \U000026A0");
                                }
                            }
                            else if (text == "آپدیت" + " \U0001F5A5" && lastcommand == "تنظیمات ربات" + " \U00002699")
                            {
                                try
                                {
                                    updatenow();
                                    bot.SendTextMessageAsync(chatId, "آپدیت انجام شد" + " \U00002705");
                                    bot.SendTextMessageAsync(ChatId_Channel, "آپدیت انجام شد" + " \U00002705");
                                }
                                catch
                                {
                                    bot.SendTextMessageAsync(chatId, "خطا در آپدیت" + " \U000026A0");
                                }
                            }
                            #region فایل نرم افزار
                            else if (text == "فایل نرم افزار" + " \U0001F4BB" && lastcommand == "تنظیمات ربات" + " \U00002699")
                            {
                                bot.SendTextMessageAsync(chatId, "فایل را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (up.Message.Type == MessageType.Document && lastcommand == "فایل نرم افزار" + " \U0001F4BB")
                            {
                                Fileid = up.Message.Document.FileId;
                                SettingsRepository.UpdateSettings(stopbot, pused, QuestionByQuestion, AnswerSheet, Fileid, AutoBackUp);
                                bot.SendTextMessageAsync(chatId, "فایل جدید قرار داده شده" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                bot.SendTextMessageAsync(ChatId_Channel, "فایل جدید قرار داده شده" + " \U00002705");
                                adder("/start", chatId);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "فایل نرم افزار" + " \U0001F4BB")
                            {
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.BotSettings);
                                adder("تنظیمات ربات" + " \U00002699", chatId);
                            }
                            #endregion
                            #region بکاپ گیری خودکار
                            else if (text == "بکاپ گیری خودکار" + " \U0001F4BE" && lastcommand == "تنظیمات ربات" + " \U00002699")
                            {
                                TimeSpan timeSpan = TimeSpan.FromSeconds(lefttime);
                                bot.SendTextMessageAsync(chatId, "\U0000231B " + "زمان باقی مانده تا بکاپ گیری خودکار :  " + timeSpan.Hours.ToString("D2") + ":" + timeSpan.Minutes.ToString("D2") + ":" + timeSpan.Seconds.ToString("D2"));
                                ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                KeyboardMarkup.ResizeKeyboard = true;
                                if (AutoBackUp)
                                {
                                    KeyboardButton[] row1 =
                                    {
                                  new KeyboardButton("متوقف کردن بکاپ گیری خودکار" + " \U000023F9") ,new KeyboardButton("تنظیم ساعت تایمر"+ " \U000023F1")
                            };
                                    KeyboardButton[] row2 =
                                    {
                                 new KeyboardButton( "بازگشت" + " \U0001F519")
                            };
                                    KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                        {
                            row1,row2
                                        };
                                }
                                else
                                {
                                    KeyboardButton[] row1 =
                                    {
                                  new KeyboardButton("فعال کردن بکاپ گیری خودکار" + " \U000025B6") ,new KeyboardButton("تنظیم ساعت تایمر"+ " \U000023F1")
                            };
                                    KeyboardButton[] row2 =
                                    {
                                 new KeyboardButton( "بازگشت" + " \U0001F519")
                            };
                                    KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                        {
                            row1,row2
                                        };
                                }
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text == "متوقف کردن بکاپ گیری خودکار" + " \U000023F9" && lastcommand == "بکاپ گیری خودکار" + " \U0001F4BE")
                            {
                                AutoBackUp = false;
                                SettingsRepository.UpdateSettings(stopbot, pused, QuestionByQuestion, AnswerSheet, Fileid, AutoBackUp);
                                ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                KeyboardMarkup.ResizeKeyboard = true;
                                KeyboardButton[] row1 =
                                    {
                                  new KeyboardButton("فعال کردن بکاپ گیری خودکار" + " \U000025B6") ,new KeyboardButton("تنظیم ساعت تایمر"+ " \U000023F1")
                            };
                                KeyboardButton[] row2 =
                                {
                                 new KeyboardButton( "بازگشت" + " \U0001F519")
                            };
                                KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                    {
                            row1,row2
                                    };
                                bot.SendTextMessageAsync(chatId, "تایمر متوقف شد" + " \U00002705", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                bot.SendTextMessageAsync(ChatId_Channel, "تایمر متوقف شد" + " \U00002705");
                            }
                            else if (text == "فعال کردن بکاپ گیری خودکار" + " \U000025B6" && lastcommand == "بکاپ گیری خودکار" + " \U0001F4BE")
                            {
                                AutoBackUp = true;
                                SettingsRepository.UpdateSettings(stopbot, pused, QuestionByQuestion, AnswerSheet, Fileid, AutoBackUp);
                                ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                KeyboardMarkup.ResizeKeyboard = true;
                                KeyboardButton[] row1 =
                                {
                                  new KeyboardButton("متوقف کردن بکاپ گیری خودکار" + " \U000023F9") ,new KeyboardButton("تنظیم ساعت تایمر"+ " \U000023F1")
                            };
                                KeyboardButton[] row2 =
                                {
                                 new KeyboardButton( "بازگشت" + " \U0001F519")
                            };
                                KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                    {
                            row1,row2
                                    };
                                bot.SendTextMessageAsync(chatId, "تایمر فعال شد" + " \U00002705", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                bot.SendTextMessageAsync(ChatId_Channel, "تایمر فعال شد" + " \U00002705");
                            }
                            else if (text == "تنظیم ساعت تایمر بکاپ گیری خودکار" + " \U000023F1" && lastcommand == "بکاپ گیری خودکار" + " \U0001F4BE")
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine("زمان مورد نظر خود را برای بکاپ گیری خودکار با فرمت زیر ارسال کنید");
                                sb.AppendLine("23:59:59");
                                sb.AppendLine("زمان نباید بیش از 24 ساعت باشد");
                                bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "تنظیم ساعت تایمر بکاپ گیری خودکار" + " \U000023F1")
                            {
                                int time;
                                try
                                {
                                    if (int.Parse(text.Trim(' ').Trim().Split(':')[0]) < 24)
                                    {
                                        time = int.Parse(text.Trim(' ').Trim().Split(':')[0]) * 60 * 60;
                                    }
                                    else
                                    {
                                        bot.SendTextMessageAsync(chatId, "ساعت نمیتواند بیش از 23 باشد" + " \U00002757");
                                        continue;
                                    }
                                    if (int.Parse(text.Trim(' ').Trim().Split(':')[1]) < 60)
                                    {
                                        time += int.Parse(text.Trim(' ').Trim().Split(':')[1]) * 60;
                                    }
                                    else
                                    {
                                        bot.SendTextMessageAsync(chatId, "دقیقه نمیتواند بیش از 59 باشد" + " \U00002757");
                                        continue;
                                    }
                                    if (int.Parse(text.Trim(' ').Trim().Split(':')[2]) < 60)
                                    {
                                        time += int.Parse(text.Trim(' ').Trim().Split(':')[2]);
                                    }
                                    else
                                    {
                                        bot.SendTextMessageAsync(chatId, "ثانیه نمی تواند بیش از 59 باشد" + " \U00002757");
                                        continue;
                                    }
                                }
                                catch
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا فرمت را درست وارد نمایید" + " \U00002757");
                                    continue;
                                }
                                lefttime = time;
                                MainTime = time;
                                ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                KeyboardMarkup.ResizeKeyboard = true;
                                if (AutoBackUp)
                                {
                                    KeyboardButton[] row1 =
                                    {
                                  new KeyboardButton("متوقف کردن بکاپ گیری خودکار" + " \U000023F9") ,new KeyboardButton("تنظیم ساعت تایمر"+ " \U000023F1")
                            };
                                    KeyboardButton[] row2 =
                                    {
                                 new KeyboardButton( "بازگشت" + " \U0001F519")
                            };
                                    KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                        {
                            row1,row2
                                        };
                                }
                                else
                                {
                                    KeyboardButton[] row1 =
                                    {
                                  new KeyboardButton("فعال کردن بکاپ گیری خودکار" + " \U000025B6") ,new KeyboardButton("تنظیم ساعت تایمر"+ " \U000023F1")
                            };
                                    KeyboardButton[] row2 =
                                    {
                                 new KeyboardButton( "بازگشت" + " \U0001F519")
                            };
                                    KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                        {
                            row1,row2
                                        };
                                }
                                bot.SendTextMessageAsync(chatId, "ساعت تنظیم شد" + " \U00002705", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                bot.SendTextMessageAsync(ChatId_Channel, "ساعت : " + text + Environment.NewLine + " برای حذف خودکار تنظیم شد" + " \U00002705");
                                adder("بکاپ گیری خودکار" + " \U0001F4BE", chatId);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "تنظیم ساعت تایمر بکاپ گیری خودکار" + " \U000023F1")
                            {
                                TimeSpan timeSpan = TimeSpan.FromSeconds(lefttime);
                                bot.SendTextMessageAsync(chatId, "\U0000231B " + "زمان باقی مانده تا بکاپ گیری خودکار :  " + timeSpan.Hours.ToString("D2") + ":" + timeSpan.Minutes.ToString("D2") + ":" + timeSpan.Seconds.ToString("D2"));
                                ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                KeyboardMarkup.ResizeKeyboard = true;
                                if (AutoBackUp)
                                {
                                    KeyboardButton[] row1 =
                                    {
                                  new KeyboardButton("متوقف کردن بکاپ گیری خودکار" + " \U000023F9") ,new KeyboardButton("تنظیم ساعت تایمر"+ " \U000023F1")
                            };
                                    KeyboardButton[] row2 =
                                    {
                                 new KeyboardButton( "بازگشت" + " \U0001F519")
                            };
                                    KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                        {
                            row1,row2
                                        };
                                }
                                else
                                {
                                    KeyboardButton[] row1 =
                                    {
                                  new KeyboardButton("فعال کردن بکاپ گیری خودکار" + " \U000025B6") ,new KeyboardButton("تنظیم ساعت تایمر"+ " \U000023F1")
                            };
                                    KeyboardButton[] row2 =
                                    {
                                 new KeyboardButton( "بازگشت" + " \U0001F519")
                            };
                                    KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                        {
                            row1,row2
                                        };
                                }
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                adder("بکاپ گیری خودکار" + " \U0001F4BE", chatId);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "بکاپ گیری خودکار" + " \U0001F4BE")
                            {
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.BotSettings);
                                adder("تنظیمات ربات" + " \U00002699", chatId);
                            }
                            #endregion
                            else if (text == "متوقف کردن ربات" + " \U0001F512" && lastcommand == "تنظیمات ربات" + " \U00002699")
                            {
                                stopbot = true;
                                SettingsRepository.UpdateSettings(stopbot, pused, QuestionByQuestion, AnswerSheet, Fileid, AutoBackUp);
                                bot.SendTextMessageAsync(chatId, "ربات متوقف شد" + " \U00002705");
                                bot.SendTextMessageAsync(ChatId_Channel, "ربات متوقف شد" + " \U00002705");
                            }
                            else if (text == "راه اندازی ربات" + " \U0001F513" && lastcommand == "تنظیمات ربات" + " \U00002699")
                            {
                                stopbot = false;
                                SettingsRepository.UpdateSettings(stopbot, pused, QuestionByQuestion, AnswerSheet, Fileid, AutoBackUp);
                                bot.SendTextMessageAsync(chatId, "ربات راه اندازی شد" + " \U00002705");
                                bot.SendTextMessageAsync(ChatId_Channel, "ربات راه اندازی شد" + " \U00002705");
                            }
                            else if (text == "بستن برنامه" + " \U0000274C" && lastcommand == "تنظیمات ربات" + " \U00002699")
                            {
                                closefrom = true;
                                bot.SendTextMessageAsync(chatId, "برنامه بسته شد" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                bot.SendTextMessageAsync(ChatId_Channel, "برنامه بسته شد" + " \U00002705");
                                adder("/start", chatId);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "تنظیمات ربات" + " \U00002699")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                            #endregion

                            #region مدیریت تراکنش ها
                            else if (text == "مدیریت تراکنش ها" + " \U0001F9FE" && lastcommand == "/start")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in TransactionsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("میلغ : " + row[1] + " تومان");
                                    stringBuilder.AppendLine("چت آیدی : " + row[2]);
                                    stringBuilder.AppendLine("کد رهگیری : " + row[3]);
                                    stringBuilder.AppendLine("بابت : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("توضیحات : " + row[6] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F465 لیست 10 تراکنش اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.TransactionsManagement);
                                adder(text, chatId);
                            }
                            #region حذف تراکنش
                            else if (text == "حذف تراکنش" + " \U0001F5D1" && lastcommand == "مدیریت تراکنش ها" + " \U0001F9FE")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in TransactionsRepository.SelectAll().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("میلغ : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[2]);
                                    stringBuilder.AppendLine("کد رهگیری : " + row[3]);
                                    stringBuilder.AppendLine("بابت : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("توضیحات : " + row[6] + Environment.NewLine + Environment.NewLine);
                                }
                                bot.SendTextMessageAsync(chatId, "\U0001F9FE لیست همه تراکنش ها : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "کد تراکنش مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "حذف تراکنش" + " \U0001F5D1")
                            {
                                if (TransactionsRepository.TransactionExist(long.Parse(text)))
                                {
                                    if (TransactionsRepository.Delete(long.Parse(text)))
                                    {
                                        bot.SendTextMessageAsync(chatId, "تراکنش با موفقیت حذف شد", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                    }
                                    else
                                    {
                                        bot.SendTextMessageAsync(chatId, "خطا در ثبت تراکنش", ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.ManagementKeyboard);
                                    }
                                    adder("/start", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "تراکنشی با این کد یافت نشد");
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "حذف تراکنش" + " \U0001F5D1")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in TransactionsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("میلغ : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[2]);
                                    stringBuilder.AppendLine("کد رهگیری : " + row[3]);
                                    stringBuilder.AppendLine("بابت : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("توضیحات : " + row[6] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F465 لیست 10 تراکنش اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.TransactionsManagement);
                                adder("مدیریت تراکنش ها" + " \U0001F9FE", chatId);
                            }
                            #endregion
                            #region افزودن تراکنش
                            else if (text == "افزودن تراکنش" + " \U00002795" && lastcommand == "مدیریت تراکنش ها" + " \U0001F9FE")
                            {
                                StringBuilder userslist = UsersList();
                                bot.SendTextMessageAsync(chatId, userslist.ToString(), ParseMode.MarkdownV2);
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine("اطلاعات تراکنش را به صورت خط به خط، به صورت زیر ارسال کنید");
                                stringBuilder.AppendLine("مبلغ" + " (اجباری) " + "- (تومان)");
                                stringBuilder.AppendLine("چت آیدی" + " (اجباری) ");
                                stringBuilder.AppendLine("کد رهگیری");
                                stringBuilder.AppendLine("بابت");
                                stringBuilder.AppendLine("تاریخ" + " (اجباری) " + "- (" + DateTime.Now.ToString() + ")");
                                stringBuilder.AppendLine("توضیحات");
                                stringBuilder.AppendLine("Finish" + " (اجباری) ");
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "افزودن تراکنش" + " \U00002795")
                            {
                                try
                                {
                                    string amount, UserChatId, TrackingCode, About, Datestring, Description;
                                    StringReader sr = new StringReader(text);
                                    amount = sr.ReadLine();
                                    UserChatId = sr.ReadLine();
                                    TrackingCode = sr.ReadLine();
                                    About = sr.ReadLine();
                                    Datestring = sr.ReadLine();
                                    Description = sr.ReadLine();
                                    string Finish = sr.ReadLine();
                                    if (TrackingCode == "")
                                        TrackingCode = "0";
                                    if (Datestring == "" || Finish == null)
                                    {
                                        bot.SendTextMessageAsync(chatId, "لطفا فرمت ها را درست وارد نمایید" + " \U00002757");
                                        continue;
                                    }
                                    if (TransactionsRepository.Insert(long.Parse(amount), long.Parse(UserChatId), long.Parse(TrackingCode), About, Convert.ToDateTime(Datestring), Description))
                                    {
                                        if (UserRepository.UpdateAmount(long.Parse(UserChatId), long.Parse(amount)))
                                        {
                                            bot.SendTextMessageAsync(UserChatId, "تراکنش جدید به مبلغ " + amount + "تومان برای شما ثبت شد");
                                            bot.SendTextMessageAsync(chatId, "تراکنش جدید ثبت شد", ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.TransactionsManagement);
                                        }
                                        else
                                            bot.SendTextMessageAsync(chatId, "خطا در تغییر موجودی", ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.TransactionsManagement);
                                    }
                                    else
                                        bot.SendTextMessageAsync(chatId, "خطا در ثبت تراکنش", ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.TransactionsManagement);
                                    adder("مدیریت تراکنش ها" + " \U0001F9FE", chatId);
                                }
                                catch
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا فرمت ها را درست وارد نمایید" + " \U00002757");
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "افزودن تراکنش" + " \U00002795")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in TransactionsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("میلغ : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[2]);
                                    stringBuilder.AppendLine("کد رهگیری : " + row[3]);
                                    stringBuilder.AppendLine("بابت : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("توضیحات : " + row[6] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F465 لیست 10 تراکنش اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.TransactionsManagement);
                                adder("مدیریت تراکنش ها" + " \U0001F9FE", chatId);
                            }
                            #endregion
                            #region جست و جو بر اساس چت آیدی
                            else if (text == "جست و جو بر اساس چت آیدی" + " \U0001F50D" && lastcommand == "مدیریت تراکنش ها" + " \U0001F9FE")
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder("TransactionsSearch", chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "TransactionsSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in TransactionsRepository.SelectByChatId(long.Parse(text)).Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("میلغ : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[2]);
                                    stringBuilder.AppendLine("کد رهگیری : " + row[3]);
                                    stringBuilder.AppendLine("بابت : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("توضیحات : " + row[6] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                    adder("/start", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "TransactionsSearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in TransactionsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("میلغ : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[2]);
                                    stringBuilder.AppendLine("کد رهگیری : " + row[3]);
                                    stringBuilder.AppendLine("بابت : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("توضیحات : " + row[6] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F465 لیست 10 تراکنش اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.TransactionsManagement);
                                adder("مدیریت تراکنش ها" + " \U0001F9FE", chatId);
                            }
                            #endregion
                            else if (text == "همه تراکنش ها" + " \U0001F9FE" && lastcommand == "مدیریت تراکنش ها" + " \U0001F9FE")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in TransactionsRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("میلغ : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[2]);
                                    stringBuilder.AppendLine("کد رهگیری : " + row[3]);
                                    stringBuilder.AppendLine("بابت : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("توضیحات : " + row[6] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    File.WriteAllText(Environment.CurrentDirectory + "\\Report.txt", stringBuilder.ToString());
                                    FileStream fs = System.IO.File.OpenRead(Environment.CurrentDirectory + "\\Report.txt");
                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, "همه تراکنش ها.txt");
                                    bot.SendDocumentAsync(chatId, inputOnlineFile);
                                    File.Delete(Environment.CurrentDirectory + "\\Report.txt");
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.TransactionsManagement);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "مدیریت تراکنش ها" + " \U0001F9FE")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                            #endregion

                            #region مدیریت پرداخت ها
                            else if (text == "مدیریت پرداخت ها" + " \U0001F4B3" && lastcommand == "/start")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in PayRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد رهگیری : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("مبلغ : " + row[2] + " تومان");
                                    stringBuilder.AppendLine("نام کاربری : " + row[3]);
                                    stringBuilder.AppendLine("نام : " + row[4]);
                                    stringBuilder.AppendLine("شماره تلفن : " + row[5]);
                                    stringBuilder.AppendLine("شماره کارت : " + row[6]);
                                    stringBuilder.AppendLine("تاریخ : " + row[7]);
                                    stringBuilder.AppendLine("توضیحات : " + row[8] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F465 لیست 10 پرداخت اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.PayManagement);
                                adder(text, chatId);
                            }
                            #region حذف رسید پرداخت
                            else if (text == "حذف رسید پرداخت" + " \U0001F5D1" && lastcommand == "مدیریت پرداخت ها" + " \U0001F4B3")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in PayRepository.SelectAll().Rows)
                                {
                                    stringBuilder.AppendLine("کد رهگیری : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("مبلغ : " + row[2] + " تومان");
                                    stringBuilder.AppendLine("نام کاربری : " + row[3]);
                                    stringBuilder.AppendLine("نام : " + row[4]);
                                    stringBuilder.AppendLine("شماره تلفن : " + row[5]);
                                    stringBuilder.AppendLine("شماره کارت : " + row[6]);
                                    stringBuilder.AppendLine("تاریخ : " + row[7]);
                                    stringBuilder.AppendLine("توضیحات : " + row[8] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    bot.SendTextMessageAsync(chatId, "\U000023F0 لیست همه پرداخت ها : " + Environment.NewLine + stringBuilder.ToString(), ParseMode.Default, false, false, 0, Keyboards.QuestionManagement);
                                    bot.SendTextMessageAsync(chatId, "کد تراکنش مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                                adder(text, chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "حذف رسید پرداخت" + " \U0001F5D1")
                            {
                                if (PayRepository.PayExist(long.Parse(text)))
                                {
                                    if (PayRepository.Delete(long.Parse(text)))
                                    {
                                        bot.SendTextMessageAsync(chatId, "رسید پرداخت با موفقیت حذف شد", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                    }
                                    else
                                    {
                                        bot.SendTextMessageAsync(chatId, "خطا در ثبت رسید پرداخت", ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.ManagementKeyboard);
                                    }
                                    adder("/start", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "رسید پرداختی با این کد یافت نشد");
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "حذف رسید پرداخت" + " \U0001F5D1")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in PayRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد رهگیری : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("مبلغ : " + row[2] + " تومان");
                                    stringBuilder.AppendLine("نام کاربری : " + row[3]);
                                    stringBuilder.AppendLine("نام : " + row[4]);
                                    stringBuilder.AppendLine("شماره تلفن : " + row[5]);
                                    stringBuilder.AppendLine("شماره کارت : " + row[6]);
                                    stringBuilder.AppendLine("تاریخ : " + row[7]);
                                    stringBuilder.AppendLine("توضیحات : " + row[8] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F465 لیست 10 پرداخت اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.PayManagement);
                                adder("مدیریت پرداخت ها" + " \U0001F4B3", chatId);
                            }
                            #endregion
                            #region افزودن رسید پرداخت
                            else if (text == "افزودن رسید پرداخت" + " \U00002795" && lastcommand == "مدیریت پرداخت ها" + " \U0001F4B3")
                            {
                                StringBuilder userslist = UsersList();
                                bot.SendTextMessageAsync(chatId, userslist.ToString(), ParseMode.MarkdownV2);
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine("اطلاعات پرداخت را به صورت خط به خط، به صورت زیر ارسال کنید");
                                stringBuilder.AppendLine("کد رهگیری" + " (اجباری) ");
                                stringBuilder.AppendLine("چت آیدی" + " (اجباری) ");
                                stringBuilder.AppendLine("میلغ" + " (اجباری) ");
                                stringBuilder.AppendLine("یوزنیم");
                                stringBuilder.AppendLine("نام");
                                stringBuilder.AppendLine("شماره تلفن");
                                stringBuilder.AppendLine("شماره کارت");
                                stringBuilder.AppendLine("تاریخ" + " (اجباری) " + "- (" + DateTime.Now.ToString() + ")");
                                stringBuilder.AppendLine("توضیحات");
                                stringBuilder.AppendLine("Finish" + " (اجباری) ");
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "افزودن رسید پرداخت" + " \U00002795")
                            {
                                try
                                {
                                    StringReader sr = new StringReader(text);
                                    string TrackingCode = sr.ReadLine();
                                    string UserChatId = sr.ReadLine();
                                    string Amount = sr.ReadLine();
                                    string username = sr.ReadLine();
                                    string name = sr.ReadLine();
                                    string phonenumber = sr.ReadLine();
                                    string card = sr.ReadLine();
                                    string stringdate = sr.ReadLine();
                                    string Description = sr.ReadLine();
                                    string Finish = sr.ReadLine();
                                    if (TrackingCode == "" || UserChatId == "" || Amount == "" || Finish == null || stringdate == "")
                                    {
                                        bot.SendTextMessageAsync(chatId, "لطفا فرمت ها را درست وارد نمایید" + " \U00002757");
                                        continue;
                                    }
                                    if (PayRepository.Insert(long.Parse(TrackingCode), long.Parse(UserChatId), long.Parse(Amount), username, name, phonenumber, card, Convert.ToDateTime(stringdate), Description))
                                    {
                                        UserRepository.UpdateAmount(long.Parse(UserChatId), long.Parse(Amount));
                                        bot.SendTextMessageAsync(UserChatId, "پرداخت شما به کد پیگیری " + TrackingCode + " و مبلغ تومان " + Amount + " ثبت شد");
                                        bot.SendTextMessageAsync(chatId, "پرداخت جدید ثبت شد", ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.PayManagement);
                                    }
                                    else
                                        bot.SendTextMessageAsync(chatId, "خطا در ثبت رسید پرداخت", ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.PayManagement);
                                    if (TransactionsRepository.Insert(long.Parse(Amount), long.Parse(UserChatId), long.Parse(TrackingCode), "خرید", Convert.ToDateTime(stringdate), ""))
                                    {
                                        if (!UserRepository.UpdateAmount(long.Parse(UserChatId), long.Parse(Amount)))
                                            bot.SendTextMessageAsync(chatId, "خطا در تغییر موجودی", ParseMode.MarkdownV2, false, false, up.Message.MessageId, Keyboards.PayManagement);
                                    }
                                    else
                                        bot.SendTextMessageAsync(chatId, "خطا در ثبت تراکنش");
                                    adder("مدیریت پرداخت ها" + " \U0001F4B3", chatId);
                                }
                                catch
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا فرمت ها را درست وارد نمایید" + " \U00002757");
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "افزودن رسید پرداخت" + " \U00002795")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in PayRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد رهگیری : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("مبلغ : " + row[2] + " تومان");
                                    stringBuilder.AppendLine("نام کاربری : " + row[3]);
                                    stringBuilder.AppendLine("نام : " + row[4]);
                                    stringBuilder.AppendLine("شماره تلفن : " + row[5]);
                                    stringBuilder.AppendLine("شماره کارت : " + row[6]);
                                    stringBuilder.AppendLine("تاریخ : " + row[7]);
                                    stringBuilder.AppendLine("توضیحات : " + row[8] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F465 لیست 10 پرداخت اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.PayManagement);
                                adder("مدیریت پرداخت ها" + " \U0001F4B3", chatId);
                            }
                            #endregion
                            #region جست و جو بر اساس چت آیدی
                            else if (text == "جست و جو بر اساس چت آیدی" + " \U0001F50D" && lastcommand == "مدیریت پرداخت ها" + " \U0001F4B3")
                            {
                                StringBuilder stringBuilder = UsersList();
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2, false, false, 0, null);
                                bot.SendTextMessageAsync(chatId, "چت آیدی کاربر مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackMainKeyboardMarkup);
                                adder("PaySearch", chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "PaySearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in PayRepository.SelectByChatId(long.Parse(text)).Rows)
                                {
                                    stringBuilder.AppendLine("کد رهگیری : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("مبلغ : " + row[2] + " تومان");
                                    stringBuilder.AppendLine("نام کاربری : " + row[3]);
                                    stringBuilder.AppendLine("نام : " + row[4]);
                                    stringBuilder.AppendLine("شماره تلفن : " + row[5]);
                                    stringBuilder.AppendLine("شماره کارت : " + row[6]);
                                    stringBuilder.AppendLine("تاریخ : " + row[7]);
                                    stringBuilder.AppendLine("توضیحات : " + row[8] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                    adder("/start", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "PaySearch")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in PayRepository.SelectTop().Rows)
                                {
                                    stringBuilder.AppendLine("کد رهگیری : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("مبلغ : " + row[2] + " تومان");
                                    stringBuilder.AppendLine("نام کاربری : " + row[3]);
                                    stringBuilder.AppendLine("نام : " + row[4]);
                                    stringBuilder.AppendLine("شماره تلفن : " + row[5]);
                                    stringBuilder.AppendLine("شماره کارت : " + row[6]);
                                    stringBuilder.AppendLine("تاریخ : " + row[7]);
                                    stringBuilder.AppendLine("توضیحات : " + row[8] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F465 لیست 10 پرداخت اخیر : " + Environment.NewLine + stringBuilder.ToString());
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.PayManagement);
                                adder("مدیریت پرداخت ها" + " \U0001F4B3", chatId);
                            }
                            #endregion
                            else if (text == "همه پرداخت ها" + " \U0001F4B3" && lastcommand == "مدیریت پرداخت ها" + " \U0001F4B3")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in PayRepository.SelectAll().Rows)
                                {
                                    stringBuilder.AppendLine("کد رهگیری : " + row[0]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("مبلغ : " + row[2] + " تومان");
                                    stringBuilder.AppendLine("نام کاربری : " + row[3]);
                                    stringBuilder.AppendLine("نام : " + row[4]);
                                    stringBuilder.AppendLine("شماره تلفن : " + row[5]);
                                    stringBuilder.AppendLine("شماره کارت : " + row[6]);
                                    stringBuilder.AppendLine("تاریخ : " + row[7]);
                                    stringBuilder.AppendLine("توضیحات : " + row[8] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    File.WriteAllText(Environment.CurrentDirectory + "\\Report.txt", stringBuilder.ToString());
                                    FileStream fs = System.IO.File.OpenRead(Environment.CurrentDirectory + "\\Report.txt");
                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, "همه پرداخت ها.txt");
                                    bot.SendDocumentAsync(chatId, inputOnlineFile);
                                    File.Delete(Environment.CurrentDirectory + "\\Report.txt");
                                }
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                                bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, Keyboards.PayManagement);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "مدیریت پرداخت ها" + " \U0001F4B3")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                            #endregion

                            else if (text != "بازگشت" + " \U0001F519" && lastcommand.StartsWith("Reply-"))
                            {
                                if (up.Message.Type == MessageType.Text)
                                {
                                    var mess = bot.SendTextMessageAsync(lastcommand.Replace("Reply-", string.Empty).Split('-')[0], "\U0001F4E9 پیام جدید از طرف ادمین :" + Environment.NewLine + text, ParseMode.Default, false, false, int.Parse(lastcommand.Replace("Reply-", string.Empty).Split('-')[1]));
                                    MessagesRepository.Insert(chatId, long.Parse(lastcommand.Replace("Reply-", string.Empty).Split('-')[0]), mess.Result.MessageId, text, DateTime.Now, int.Parse(lastcommand.Replace("Reply-", string.Empty).Split('-')[1]), "شخصی");
                                    bot.SendTextMessageAsync(chatId, "پیام به کاربر [" + lastcommand.Replace("Reply-", string.Empty).Split('-')[0] + "](tg://user?id=" + lastcommand.Replace("Reply-", string.Empty).Split('-')[0] + ") ارسال شد" + " \U00002705", ParseMode.MarkdownV2, false, false, int.Parse(lastcommand.Replace("Reply-", string.Empty).Split('-')[2]), Keyboards.ManagementKeyboard);
                                    var messCH = bot.SendTextMessageAsync(ChatId_Channel, text).Result;
                                    bot.SendTextMessageAsync(ChatId_Channel, "پیام به کاربر [" + lastcommand.Replace("Reply-", string.Empty).Split('-')[0] + "](tg://user?id=" + lastcommand.Replace("Reply-", string.Empty).Split('-')[0] + ") ارسال شد" + " \U00002705", ParseMode.MarkdownV2, false, false, messCH.MessageId);
                                    adder("/start", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا فقط پیام متنی ارسال کنید");
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand.StartsWith("Reply-"))
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                            else if (text == "خروج از پنل کاربری" + " \U0000274C" && lastcommand == "/start")
                            {
                                UserRepository.UpdatePanel(chatId, signuped.Replace("Management-", string.Empty));
                                ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                KeyboardMarkup.ResizeKeyboard = true;
                                if (signuped.Replace("Management-", string.Empty) == "")
                                {
                                    if (!named)
                                    {
                                        KeyboardMarkup = Keyboards.SignupKeyboardMarkup;
                                    }
                                    else if (phone == "")
                                    {
                                        KeyboardMarkup = Keyboards.signupedKeyboardMarkup;
                                    }
                                    else
                                    {
                                        KeyboardMarkup = Keyboards.finishKeyboardMarkup;
                                    }
                                }
                                else if (signuped.Replace("Management-", string.Empty) == "Verified")
                                {
                                    KeyboardMarkup = Keyboards.VerifiedKeyboardMarkup;
                                }
                                else if (signuped.Replace("Management-", string.Empty) == "Blocked")
                                {
                                    KeyboardMarkup = Keyboards.BlockedKeyboardMarkup;
                                }
                                bot.SendTextMessageAsync(chatId, "شما از پنل مدیریت خارج شدید" + " \U00002705", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                adder("/start", chatId);
                            }
                            else
                            {
                                bot.SendTextMessageAsync(chatId, "من که نفهمیدم چی میگی" + " \U0001F914", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                                adder("/start", chatId);
                            }
                        }
                        else if (signuped == "")
                        {
                            if (!named)
                            {
                                if (text == "/start")
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("به ربات ما ما خوش آمدید");
                                    sb.AppendLine("برای ادامه کار با ربات ثبت نام کنید !");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.SignupKeyboardMarkup);
                                    adder(text, chatId);
                                }
                                else if (text == "ارتباط با ما" + " \U0001F4AC" && lastcommand == "/start")
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا پیام خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                    adder(text, chatId);
                                }
                                else if (text == "ثبت نام" + " \U0000270D" && lastcommand == "/start")
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا نام و نام خانوادگی خود را وارد کنید.", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                    adder(text, chatId);
                                }
                                else if (text != "بازگشت" + " \U0001F519" && lastcommand == "ارتباط با ما" + " \U0001F4AC")
                                {
                                    var keynboard = new InlineKeyboardMarkup(new[]
                                    {
                                    new[]
                                    {
                                        new InlineKeyboardButton()
                                        {
                                            Text = "پاسخ دادن",
                                            CallbackData = chatId.ToString() + "-" + up.Message.MessageId
                                        }
                                    }
                                    });
                                    var formess = bot.ForwardMessageAsync(CahtId_Admin, chatId, up.Message.MessageId);
                                    MessagesRepository.Insert(chatId, CahtId_Admin, formess.Result.MessageId, text, DateTime.Now, 0, "شخصی");
                                    var formesschan = bot.ForwardMessageAsync(ChatId_Channel, chatId, up.Message.MessageId);
                                    bot.SendTextMessageAsync(CahtId_Admin, "\U0001F4E9 " + "پیام جدید از طرف : [" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ")", ParseMode.MarkdownV2, false, false, formess.Result.MessageId, keynboard);
                                    bot.SendTextMessageAsync(ChatId_Channel, "\U0001F4E9 " + "پیام جدید از طرف : [" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ")", ParseMode.MarkdownV2, false, false, formesschan.Result.MessageId);
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("پیام ارسال شد" + " \U00002705");
                                    sb.AppendLine("برای ادامه کار با ربات ثبت نام کنید !");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.SignupKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                                else if (text != "بازگشت" + " \U0001F519" && lastcommand == "ثبت نام" + " \U0000270D")
                                {
                                    bot.SendTextMessageAsync(CahtId_Admin, "[" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ") با نام " + text + " در ربات ثبت نام کرد", ParseMode.MarkdownV2);
                                    bot.SendTextMessageAsync(ChatId_Channel, "[" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ") با نام " + text + " در ربات ثبت نام کرد", ParseMode.MarkdownV2);
                                    UserRepository.ChangeName(chatId, text);
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("\U0001F4F1 " + "برای ادامه فرایند ثبت نام نیاز به شماره تلفن شما است لطفا روی کلید ارسال شماره تلفن بزنید");
                                    sb.AppendLine("\U000026A0 " + "تمامی اطلاعات شما شما نزد ما محفوط خواهد بود");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.signupedKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                                else if (text == "بازگشت" + " \U0001F519" && lastcommand == "ثبت نام" + " \U0000270D")
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("صفحه اصلی" + " \U0001F3E0");
                                    sb.AppendLine("برای ادامه کار با ربات ثبت نام کنید !");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.SignupKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                                else if (text == "بازگشت" + " \U0001F519" && lastcommand == "ارتباط با ما" + " \U0001F4AC")
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("صفحه اصلی" + " \U0001F3E0");
                                    sb.AppendLine("برای ادامه کار با ربات ثبت نام کنید !");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.SignupKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "من که نفهمیدم چی میگی" + " \U0001F914", ParseMode.Default, false, false, 0, Keyboards.SignupKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                            }
                            else if (phone == "")
                            {
                                if (text == "/start")
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("\U0001F4F1 " + "برای ادامه فرایند ثبت نام نیاز به شماره تلفن شما است لطفا روی کلید ارسال شماره تلفن بزنید");
                                    sb.AppendLine("\U000026A0 " + "تمامی اطلاعات شما شما نزد ما محفوط خواهد بود");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.signupedKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                                else if (up.Message.Contact != null && lastcommand == "/start")
                                {
                                    if (up.Message.Contact.UserId == chatId)
                                    {
                                        bot.SendTextMessageAsync(CahtId_Admin, "[" + chatId + "](tg://user?id=" + chatId + ") با شماره تلفن " + up.Message.Contact.PhoneNumber + " در ربات ثبت نام کرد", ParseMode.MarkdownV2);
                                        bot.SendTextMessageAsync(ChatId_Channel, "[" + chatId + "](tg://user?id=" + chatId + ") با شماره تلفن " + up.Message.Contact.PhoneNumber + " در ربات ثبت نام کرد", ParseMode.MarkdownV2);
                                        UserRepository.ChangePhone(chatId, up.Message.Contact.PhoneNumber.Replace('+', ' ').Trim());
                                        bot.SendTextMessageAsync(chatId, "ثبت نام شما با موفقیت انجام شد لطفا منتظر تایید ادمین باشید" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.finishKeyboardMarkup);
                                        adder("/start", chatId);
                                    }
                                    else
                                    {
                                        bot.SendTextMessageAsync(chatId, "لطفا برای ارسال شماره از دکمه ارسال شماره استفاده نمایید", ParseMode.Default, false, false, 0, null);
                                    }
                                }
                                else if (text == "ارتباط با ما" + " \U0001F4AC" && lastcommand == "/start")
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا پیام خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                    adder(text, chatId);
                                }
                                else if (text != "بازگشت" + " \U0001F519" && lastcommand == "ارتباط با ما" + " \U0001F4AC")
                                {
                                    var keynboard = new InlineKeyboardMarkup(new[]
                                   {
                                    new[]
                                    {
                                        new InlineKeyboardButton()
                                        {
                                            Text = "پاسخ دادن",
                                            CallbackData = chatId.ToString() + "-" + up.Message.MessageId
                                        }
                                    }
                                });
                                    var formess = bot.ForwardMessageAsync(CahtId_Admin, chatId, up.Message.MessageId);
                                    MessagesRepository.Insert(chatId, CahtId_Admin, formess.Result.MessageId, text, DateTime.Now, 0, "شخصی");
                                    var formesschan = bot.ForwardMessageAsync(ChatId_Channel, chatId, up.Message.MessageId);
                                    bot.SendTextMessageAsync(CahtId_Admin, "\U0001F4E9 " + "پیام جدید از طرف : [" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ")", ParseMode.MarkdownV2, false, false, formess.Result.MessageId, keynboard);
                                    bot.SendTextMessageAsync(ChatId_Channel, "\U0001F4E9 " + "پیام جدید از طرف : [" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ")", ParseMode.MarkdownV2, false, false, formesschan.Result.MessageId);
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("پیام ارسال شد" + " \U00002705");
                                    sb.AppendLine("برای ادامه کار با ربات ثبت نام کنید !");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.signupedKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                                else if (text == "بازگشت" + " \U0001F519" && lastcommand == "ارتباط با ما" + " \U0001F4AC")
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("صفحه اصلی" + " \U0001F3E0");
                                    sb.AppendLine("برای ادامه کار با ربات ثبت نام کنید !");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.signupedKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "من که نفهمیدم چی میگی" + " \U0001F914", ParseMode.Default, false, false, 0, Keyboards.finishKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                            }
                            else
                            {
                                if (text == "/start")
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("به ربات ما ما خوش آمدید");
                                    sb.AppendLine("ثبت نام شما انجام شده لطفا منتظر تایید ادمین باشید" + " \U000023F3");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.finishKeyboardMarkup);
                                    adder(text, chatId);
                                }
                                else if (text == "ارتباط با ما" + " \U0001F4AC" && lastcommand == "/start")
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا پیام خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                    adder(text, chatId);
                                }
                                else if (text != "بازگشت" + " \U0001F519" && lastcommand == "ارتباط با ما" + " \U0001F4AC")
                                {
                                    var keynboard = new InlineKeyboardMarkup(new[]
                                {
                                    new[]
                                    {
                                        new InlineKeyboardButton()
                                        {
                                            Text = "پاسخ دادن",
                                            CallbackData = chatId.ToString() + "-" + up.Message.MessageId
                                        }
                                    }
                                });
                                    var formess = bot.ForwardMessageAsync(CahtId_Admin, chatId, up.Message.MessageId);
                                    MessagesRepository.Insert(chatId, CahtId_Admin, formess.Result.MessageId, text, DateTime.Now, 0, "شخصی");
                                    var formesschan = bot.ForwardMessageAsync(ChatId_Channel, chatId, up.Message.MessageId);
                                    bot.SendTextMessageAsync(CahtId_Admin, "\U0001F4E9 " + "پیام جدید از طرف : [" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ")", ParseMode.MarkdownV2, false, false, formess.Result.MessageId, keynboard);
                                    bot.SendTextMessageAsync(ChatId_Channel, "\U0001F4E9 " + "پیام جدید از طرف : [" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ")", ParseMode.MarkdownV2, false, false, formesschan.Result.MessageId);
                                    bot.SendTextMessageAsync(chatId, "پیام ارسال شد" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.finishKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                                else if (text == "بازگشت" + " \U0001F519" && lastcommand == "ارتباط با ما" + " \U0001F4AC")
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("صفحه اصلی" + " \U0001F3E0");
                                    sb.AppendLine("ثبت نام شما انجام شده لطفا منتظر تایید ادمین باشید" + " \U000023F3");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.finishKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "من که نفهمیدم چی میگی" + " \U0001F914", ParseMode.Default, false, false, 0, Keyboards.finishKeyboardMarkup);
                                    adder("/start", chatId);
                                }
                            }
                        }
                        else if (signuped == "Verified")
                        {
                            if (text == "/start")
                            {
                                bot.SendTextMessageAsync(chatId, "به ربات ما خوش آمدید", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                adder("/start", chatId);
                            }
                            else if (text == "دریافت نرم افزار برای ویندوز" + " \U0001F4BB" && lastcommand == "/start")
                            {
                                if (Fileid != "")
                                {
                                    bot.SendTextMessageAsync(chatId, "در حال ارسال فایل ..." + " \U000023F3", ParseMode.Default);
                                    ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                    KeyboardMarkup.ResizeKeyboard = true;
                                    KeyboardButton[] row1 =
                                    {
                                  new KeyboardButton("دریافت نرم افزار برای ویندوز" + " \U0001F4BB"), new KeyboardButton("دریافت آزمون" + " \U0001F4E5")
                            };
                                    KeyboardButton[] row2 =
                                    {
                                 new KeyboardButton("ارتباط با ما" + " \U0001F4AC")
                            };
                                    KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                        {
                            row1,row2
                                        };
                                    if (Fileid != "")
                                        sendfile(up);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "به علت عدم استقبال از نسخه ویندوزی و تمایل کاربران برای استفاده از ربات تلگرام، نسخه ویندوزی تا اطلاع ثانویه پشتیبانی نمیشود.", ParseMode.Default);
                                }

                            }
                            else if (text == "مشخصات کاربری" + " \U0001F464" && lastcommand == "/start")
                            {
                                DataTable userinfo = UserRepository.SelectRow(chatId);
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.AppendLine("چت آیدی : " + userinfo.Rows[0][0]);
                                stringBuilder.AppendLine("نام کاربری : " + up.Message.Chat.Username);
                                stringBuilder.AppendLine("نام : " + userinfo.Rows[0][1]);
                                stringBuilder.AppendLine("شماره تلفن : " + userinfo.Rows[0][4]);
                                stringBuilder.AppendLine("تاریخ عضویت : " + userinfo.Rows[0][3]);
                                stringBuilder.AppendLine("موجودی : " + long.Parse(userinfo.Rows[0][6].ToString()) + " تومان");
                                bot.SendTextMessageAsync(chatId, stringBuilder.ToString());
                            }
                            else if (text == "لیست تراکنش ها" + " \U0001F9FE" && lastcommand == "/start")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in TransactionsRepository.SelectByChatId(chatId).Rows)
                                {
                                    stringBuilder.AppendLine("کد : " + row[0]);
                                    stringBuilder.AppendLine("میلغ : " + row[1]);
                                    stringBuilder.AppendLine("چت آیدی : " + row[2]);
                                    stringBuilder.AppendLine("کد رهگیری : " + row[3]);
                                    stringBuilder.AppendLine("بابت : " + row[4]);
                                    stringBuilder.AppendLine("تاریخ : " + row[5]);
                                    stringBuilder.AppendLine("توضیحات : " + row[6] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F465 لیست تراکنش ها : " + Environment.NewLine + stringBuilder.ToString());
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                            }
                            else if (text == "لیست دانلود ها" + " \U0001F4C4" && lastcommand == "/start")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in DownloadRepository.SelectByChatId(chatId).Rows)
                                {
                                    stringBuilder.AppendLine("چت آیدی : " + row[1]);
                                    stringBuilder.AppendLine("کد آزمون : " + row[2]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[3]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[4]);
                                    stringBuilder.AppendLine("روش ارسال : " + row[5]);
                                    stringBuilder.AppendLine("ارسال توسط : " + row[6]);
                                    stringBuilder.AppendLine("تاریخ : " + row[8]);
                                    stringBuilder.AppendLine("هزینه : " + row[9] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                    bot.SendTextMessageAsync(chatId, "\U0001F465 لیست دانلود ها : " + Environment.NewLine + stringBuilder.ToString());
                                else
                                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                            }
                            #region لیست پرداخت ها
                            //else if (text == "لیست پرداخت ها" + " \U0001F4B3" && lastcommand == "/start")
                            //{
                            //    StringBuilder stringBuilder = new StringBuilder();
                            //    foreach (DataRow row in PayRepository.SelectTop().Rows)
                            //    {
                            //        stringBuilder.AppendLine("کد رهگیری : " + row[0]);
                            //        stringBuilder.AppendLine("چت آیدی : " + row[1]);
                            //        stringBuilder.AppendLine("مبلغ : " + row[2] + " تومان");
                            //        stringBuilder.AppendLine("نام کاربری : " + row[3]);
                            //        stringBuilder.AppendLine("نام : " + row[4]);
                            //        stringBuilder.AppendLine("شماره تلفن : " + row[5]);
                            //        stringBuilder.AppendLine("شماره کارت : " + row[6]);
                            //        stringBuilder.AppendLine("تاریخ : " + row[7]);
                            //        stringBuilder.AppendLine("توضیحات : " + row[8] + Environment.NewLine + Environment.NewLine);
                            //    }
                            //    if (stringBuilder.ToString() != "")
                            //        bot.SendTextMessageAsync(chatId, "\U0001F465 لیست 10 پرداخت ها : " + Environment.NewLine + stringBuilder.ToString());
                            //    else
                            //        bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                            //}
                            #endregion
                            #region افزودن موجودی
                            //else if (text == "افزودن موجودی" + " \U0001F4B3" && lastcommand == "/start")
                            //{
                            //    var BuyKeyboard = new InlineKeyboardMarkup(new[]
                            //    {
                            //        new[]
                            //        {
                            //            new InlineKeyboardButton()
                            //            {
                            //                Text = "10000 تومان",
                            //                Url = "https://idpay.ir/quizbot/100000"
                            //            },
                            //            new InlineKeyboardButton()
                            //            {
                            //                Text = "5000 تومان",
                            //                Url = "https://idpay.ir/quizbot/50000"
                            //            }
                            //        },
                            //        new[]
                            //        {
                            //            new InlineKeyboardButton()
                            //            {
                            //                Text = "30000 تومان",
                            //                Url = "https://idpay.ir/quizbot/300000"
                            //            },
                            //            new InlineKeyboardButton()
                            //            {
                            //                Text = "20000 تومان",
                            //                Url = "https://idpay.ir/quizbot/200000"
                            //            }
                            //        },
                            //        new[]
                            //        {
                            //            new InlineKeyboardButton()
                            //            {
                            //                Text = "100000 تومان",
                            //                Url = "https://idpay.ir/quizbot/1000000"
                            //            },
                            //            new InlineKeyboardButton()
                            //            {
                            //                Text = "50000 تومان",
                            //                Url = "https://idpay.ir/quizbot/500000"
                            //            }
                            //        },
                            //        new[]
                            //        {
                            //            new InlineKeyboardButton()
                            //            {
                            //                Text = "مبلغ دلخواه",
                            //                CallbackData = "Buy"
                            //            }
                            //        }
                            //    });
                            //    bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, BuyKeyboard);
                            //}
                            //else if (text == "بازگشت" + " \U0001F519" && lastcommand == "Buy")
                            //{
                            //    bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                            //    adder("/start", chatId);
                            //}
                            //else if (lastcommand == "Buy")
                            //{
                            //    try
                            //    {
                            //        int price = int.Parse(text);
                            //        var BuyKeyboard = new InlineKeyboardMarkup(new[]
                            //        {
                            //        new[]
                            //        {
                            //            new InlineKeyboardButton()
                            //            {
                            //                Text = price + " تومان",
                            //                Url = "https://idpay.ir/quizbot/" + price*10
                            //            },
                            //        }
                            //    });
                            //        bot.SendTextMessageAsync(chatId, "روی دکمه زیر کلیک کنید", ParseMode.Default, false, false, 0, BuyKeyboard);
                            //    }
                            //    catch
                            //    {
                            //        bot.SendTextMessageAsync(chatId, "لطفا فقط عدد ارسال کنید");
                            //    }
                            //}
                            #endregion
                            #region دریافت آزمون
                            //else if (text == "دریافت آزمون" + " \U0001F4E5" && lastcommand == "/start")
                            //{
                            //    StringBuilder stringBuilder = new StringBuilder();
                            //    foreach (DataRow row in ExamAccessRepository.SelectByChatId(chatId).Rows)
                            //    {
                            //        stringBuilder.AppendLine("کد آزمون : " + row[1]);
                            //        stringBuilder.AppendLine("نام آزمون : " + row[2]);
                            //        stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[4]);
                            //        stringBuilder.AppendLine("تعداد سوال : " + row[5] + Environment.NewLine + Environment.NewLine);
                            //    }
                            //    if (stringBuilder.ToString() != "")
                            //    {
                            //        bot.SendTextMessageAsync(chatId, "لیست آزمون های شما :" + Environment.NewLine + stringBuilder.ToString());
                            //        bot.SendTextMessageAsync(chatId, "کد آزمون مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                            //        bot.SendTextMessageAsync(chatId, "هزینه دانلود هر آزمون 10000 تومان است در صورتی که قصد دانلود رایگان سوالات را دارید 7 دقیقه پایانی زمان آزمون میتوانید رایگان دانلود کنید");
                            //        adder(text, chatId);
                            //    }
                            //    else
                            //    {
                            //        bot.SendTextMessageAsync(chatId, "آزمونی برای شما یافت نشد" + Environment.NewLine + " جهت دریافت راهنمایی از قسمت ارتباط با ما، به ادمین پیام ارسال کنید");
                            //    }
                            //}
                            else if (text == "دریافت آزمون" + " \U0001F4E5" && lastcommand == "/start")
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (DataRow row in ExamAccessRepository.SelectByChatId(chatId).Rows)
                                {
                                    stringBuilder.AppendLine("کد آزمون : " + row[1]);
                                    stringBuilder.AppendLine("نام آزمون : " + row[2]);
                                    stringBuilder.AppendLine("تاریخ پایان آزمون : " + row[4]);
                                    stringBuilder.AppendLine("تعداد سوال : " + row[5] + Environment.NewLine + Environment.NewLine);
                                }
                                if (stringBuilder.ToString() != "")
                                {
                                    bot.SendTextMessageAsync(chatId, "لیست آزمون های شما :" + Environment.NewLine + stringBuilder.ToString());
                                    bot.SendTextMessageAsync(chatId, "کد آزمون مورد نظر را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                    adder(text, chatId);
                                }
                                else
                                {
                                    bot.SendTextMessageAsync(chatId, "آزمونی برای شما یافت نشد" + Environment.NewLine + " جهت دریافت راهنمایی از قسمت ارتباط با ما، به ادمین پیام ارسال کنید");
                                }
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "دریافت آزمون" + " \U0001F4E5")
                            {
                                try
                                {
                                    if (ExamAccessRepository.Exist(chatId, long.Parse(text)))
                                    {
                                        long Amount = 10000;
                                        DateTime time = Convert.ToDateTime(ExamsRepository.SelectRow(long.Parse(text)).Rows[0][3]);
                                        DateTime Now = DateTime.Now;
                                        double diff = Convert.ToDouble((time - Now).TotalMinutes.ToString());
                                        int different = (int)diff;
                                        if (different <= 7)
                                            Amount = 0;
                                        if (time > Now)
                                        {
                                            if (long.Parse(UserRepository.SelectRow(chatId).Rows[0][6].ToString()) >= Amount)
                                            {
                                                if (AnswerSheet && QuestionByQuestion)
                                                {
                                                    ReplyKeyboardMarkup KeyboardMarkup = new ReplyKeyboardMarkup();
                                                    KeyboardMarkup.ResizeKeyboard = true;
                                                    KeyboardButton[] row1 =
                                                    {
                                  new KeyboardButton("دریافت پاسخنامه متنی" + " \U0001F4CB"), new KeyboardButton("دریافت سوال به سوال" + " \U0001F5BC")
                            };
                                                    KeyboardButton[] row2 =
                                                    {
                                 new KeyboardButton("بازگشت" + " \U0001F519")
                            };
                                                    KeyboardMarkup.Keyboard = new KeyboardButton[][]
                                                        {
                            row1,row2
                                                        };
                                                    bot.SendTextMessageAsync(chatId, "یکی از گزینه های زیر را انتخاب کنید", ParseMode.Default, false, false, 0, KeyboardMarkup);
                                                    adder("دریافت آزمون-" + text, chatId);
                                                }
                                                else if (!AnswerSheet && QuestionByQuestion)
                                                {
                                                    new Thread(delegate ()
                                                    {
                                                        ques(text, chatId, Amount);
                                                    }).Start();
                                                }
                                                else if (AnswerSheet && !QuestionByQuestion)
                                                {
                                                    new Thread(delegate ()
                                                    {
                                                        answ(text, chatId, Amount);
                                                    }).Start();
                                                }
                                                else if (!AnswerSheet && !QuestionByQuestion)
                                                {
                                                    bot.SendTextMessageAsync(chatId, "امکان دانلود سوالات توسط ادمین محدود شده است", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                                }
                                            }
                                            else
                                            {
                                                bot.SendTextMessageAsync(chatId, "موجودی شما کافی نمی باشد" + "هزینه دانلود هر آزمون 10000 تومان است" + Environment.NewLine + "برای افزایش موجودی به قسمت ارتباط با ما پیام ارسال کنید" + Environment.NewLine + "این ربات فعلا رایگان است و برای افزایش موجودی هزینه ای دریافت نمی شود", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                                adder("/strat", chatId);
                                            }
                                        }
                                        else
                                        {
                                            bot.SendTextMessageAsync(chatId, "زمان آزمون به اتمام رسیده است", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                            adder("/start", chatId);
                                        }
                                    }
                                    else
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        sb.AppendLine("کد وارد شده اشتباه می باشد");
                                        sb.AppendLine("لطفا مجددا تلاش نمایید");
                                        bot.SendTextMessageAsync(chatId, sb.ToString());
                                    }
                                }
                                catch
                                {
                                    bot.SendTextMessageAsync(chatId, "لطفا فقط عدد ارسال کنید");
                                }
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "دریافت آزمون" + " \U0001F4E5")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                adder("/start", chatId);
                            }
                            else if (text == "دریافت سوال به سوال" + " \U0001F5BC" && lastcommand.StartsWith("دریافت آزمون-"))
                            {
                                if (ExamAccessRepository.Exist(chatId, long.Parse(lastcommand.Split('-')[1])))
                                {
                                    long Amount = 100000;
                                    DateTime time = Convert.ToDateTime(ExamsRepository.SelectRow(long.Parse(lastcommand.Split('-')[1])).Rows[0][3]);
                                    DateTime Now = DateTime.Now;
                                    double diff = Convert.ToDouble((time - Now).TotalMinutes.ToString());
                                    int different = (int)diff;
                                    if (different <= 10)
                                        Amount = 0;
                                    if (long.Parse(UserRepository.SelectRow(chatId).Rows[0][6].ToString()) >= Amount)
                                    {
                                        new Thread(delegate ()
                                        {
                                            ques(lastcommand.Split('-')[1], chatId, Amount);
                                        }).Start();
                                    }
                                    else
                                    {
                                        bot.SendTextMessageAsync(chatId, "موجودی شما کافی نمی باشد", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                        adder("/strat", chatId);
                                    }
                                }
                                else
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("کد وارد شده اشتباه می باشد");
                                    sb.AppendLine("لطفا مجددا تلاش نمایید");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                    adder(lastcommand, chatId);
                                }
                            }
                            else if (text == "دریافت پاسخنامه متنی" + " \U0001F4CB" && lastcommand.StartsWith("دریافت آزمون-"))
                            {
                                if (ExamAccessRepository.Exist(chatId, long.Parse(lastcommand.Split('-')[1])))
                                {
                                    long Amount = 100000;
                                    DateTime time = Convert.ToDateTime(ExamsRepository.SelectRow(long.Parse(lastcommand.Split('-')[1])).Rows[0][3]);
                                    DateTime Now = DateTime.Now;
                                    double diff = Convert.ToDouble((time - Now).TotalMinutes.ToString());
                                    int different = (int)diff;
                                    if (different <= 10)
                                        Amount = 0;
                                    if (long.Parse(UserRepository.SelectRow(chatId).Rows[0][6].ToString()) >= Amount)
                                    {
                                        new Thread(delegate ()
                                        {
                                            answ(lastcommand.Split('-')[1], chatId, Amount);
                                        }).Start();
                                    }
                                    else
                                    {
                                        bot.SendTextMessageAsync(chatId, "موجودی شما کافی نمی باشد", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                        adder("/strat", chatId);
                                    }
                                }
                                else
                                {
                                    StringBuilder sb = new StringBuilder();
                                    sb.AppendLine("کد وارد شده اشتباه می باشد");
                                    sb.AppendLine("لطفا مجددا تلاش نمایید");
                                    bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                    adder(lastcommand, chatId);
                                }
                            }
                            #endregion
                            #region ارتباط با ما
                            else if (text == "ارتباط با ما" + " \U0001F4AC" && lastcommand == "/start")
                            {
                                bot.SendTextMessageAsync(chatId, "لطفا پیام خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "ارتباط با ما" + " \U0001F4AC")
                            {
                                var keynboard = new InlineKeyboardMarkup(new[]
                                {
                                    new[]
                                    {
                                        new InlineKeyboardButton()
                                        {
                                            Text = "پاسخ دادن",
                                            CallbackData = chatId.ToString() + "-" + up.Message.MessageId
                                        }
                                    }
                                });
                                var formess = bot.ForwardMessageAsync(CahtId_Admin, chatId, up.Message.MessageId);
                                MessagesRepository.Insert(chatId, CahtId_Admin, formess.Result.MessageId, text, DateTime.Now, 0, "شخصی");
                                var formesschan = bot.ForwardMessageAsync(ChatId_Channel, chatId, up.Message.MessageId);
                                bot.SendTextMessageAsync(CahtId_Admin, "\U0001F4E9 " + "پیام جدید از طرف : [" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ")", ParseMode.MarkdownV2, false, false, formess.Result.MessageId, keynboard);
                                bot.SendTextMessageAsync(ChatId_Channel, "\U0001F4E9 " + "پیام جدید از طرف : [" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ")", ParseMode.MarkdownV2, false, false, formesschan.Result.MessageId);
                                bot.SendTextMessageAsync(chatId, "پیام ارسال شد" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                adder("/start", chatId);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "ارتباط با ما" + " \U0001F4AC")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                adder("/start", chatId);
                            }
                            #endregion
                            else
                            {
                                bot.SendTextMessageAsync(chatId, "من که نفهمیدم چی میگی" + " \U0001F914", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                                adder("/start", chatId);
                            }
                        }
                        else if (signuped == "Blocked")
                        {
                            if (text == "/start")
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine("به ربات ما ما خوش آمدید");
                                sb.AppendLine("متاسفانه شما توسط ادمین بلاک شدید" + " \U000026D4");
                                bot.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Default, false, false, 0, Keyboards.BlockedKeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text == "ارتباط با ما" + " \U0001F4AC" && lastcommand == "/start")
                            {
                                bot.SendTextMessageAsync(chatId, "لطفا پیام خود را ارسال کنید", ParseMode.Default, false, false, 0, Keyboards.BackKeyboardMarkup);
                                adder(text, chatId);
                            }
                            else if (text != "بازگشت" + " \U0001F519" && lastcommand == "ارتباط با ما" + " \U0001F4AC")
                            {
                                var keynboard = new InlineKeyboardMarkup(new[]
                                   {
                                    new[]
                                    {
                                        new InlineKeyboardButton()
                                        {
                                            Text = "پاسخ دادن",
                                            CallbackData = chatId.ToString() + "-" + up.Message.MessageId
                                        }
                                    }
                                });
                                var formess = bot.ForwardMessageAsync(CahtId_Admin, chatId, up.Message.MessageId);
                                MessagesRepository.Insert(chatId, CahtId_Admin, formess.Result.MessageId, text, DateTime.Now, 0, "شخصی");
                                var formesschan = bot.ForwardMessageAsync(ChatId_Channel, chatId, up.Message.MessageId);
                                bot.SendTextMessageAsync(CahtId_Admin, "\U0001F4E9 " + "پیام جدید از طرف : [" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ")", ParseMode.MarkdownV2, false, false, formess.Result.MessageId, keynboard);
                                bot.SendTextMessageAsync(ChatId_Channel, "\U0001F4E9 " + "پیام جدید از طرف : [" + chatId + "](tg://user?id=" + up.Message.Chat.Id + ")", ParseMode.MarkdownV2, false, false, formesschan.Result.MessageId);
                                bot.SendTextMessageAsync(chatId, "پیام ارسال شد" + " \U00002705", ParseMode.Default, false, false, 0, Keyboards.BlockedKeyboardMarkup);
                                adder("/start", chatId);
                            }
                            else if (text == "بازگشت" + " \U0001F519" && lastcommand == "ارتباط با ما" + " \U0001F4AC")
                            {
                                bot.SendTextMessageAsync(chatId, "صفحه اصلی" + " \U0001F3E0", ParseMode.Default, false, false, 0, Keyboards.BlockedKeyboardMarkup);
                                adder("/start", chatId);
                            }
                            else
                            {
                                bot.SendTextMessageAsync(chatId, "من که نفهمیدم چی میگی" + " \U0001F914", ParseMode.Default, false, false, 0, Keyboards.BlockedKeyboardMarkup);
                                adder("/start", chatId);
                            }
                        }
                    }
                }
                catch { }
            }
        }
        private void UpdateExams(Telegram.Bot.Types.Update up, long chatId)
        {
            try
            {
                bot.SendTextMessageAsync(chatId, "در حال آپدیت آزمون ها ...");
                foreach (string exam in uploades().ToArray())
                {
                    if (!ExamsRepository.ExamExist(long.Parse(exam)))
                    {
                        string url = "https://quiz24exam.parsaspace.com//Exams//" + exam + ".txt";
                        WebClient webClient = new WebClient();
                        StreamReader str = new StreamReader(webClient.OpenRead(url));
                        ExamsRepository.Insert(long.Parse(exam), str.ReadLine().Split('=')[1].Trim(), DateTime.Now, DateTime.Now);
                        while (!str.EndOfStream)
                        {
                            int Id = int.Parse(str.ReadLine().Split('=')[1].Trim());
                            int HardLevel = int.Parse(str.ReadLine().Split('=')[1].Trim());
                            int Key = int.Parse(str.ReadLine().Split('=')[1].Trim());
                            string QuestionImage = str.ReadLine().Split('=')[1].Trim();
                            string AnswerImage = str.ReadLine().Split('=')[1].Trim();
                            str.ReadLine();
                            QuestionsRepository.Insert(long.Parse(exam), Id, QuestionImage, AnswerImage, Key, HardLevel);
                        }
                    }
                }
                bot.SendTextMessageAsync(chatId, "آپدیت آزمون ها انجام شد");
            }
            catch
            {
                bot.SendTextMessageAsync(chatId, "خظا در آپدیت آزمون ها");
            }
        }
        private void AssignExam(string lastcommand, long chatId, string User)
        {
            try
            {
                bot.SendTextMessageAsync(chatId, "در حال اختصاص دادن ...", ParseMode.Default, false, false, 0, Keyboards.ExamEditor);
                adder(lastcommand.Replace("Assign", "Exam"), chatId);

                if (User == "All")
                    foreach (DataRow row in UserRepository.SelectAll().Rows)
                        ExamAccessRepository.Insert(long.Parse(row[0].ToString()), long.Parse(lastcommand.Split('-')[1]));
                else if (User == "Verified")
                    foreach (DataRow row in UserRepository.SelectByPanel("Verified").Rows)
                        ExamAccessRepository.Insert(long.Parse(row[0].ToString()), long.Parse(lastcommand.Split('-')[1]));
                else if (User == "Blocked")
                    foreach (DataRow row in UserRepository.SelectByPanel("Blocked").Rows)
                        ExamAccessRepository.Insert(long.Parse(row[0].ToString()), long.Parse(lastcommand.Split('-')[1]));
                else if (User == "")
                    foreach (DataRow row in UserRepository.SelectByPanel("").Rows)
                        ExamAccessRepository.Insert(long.Parse(row[0].ToString()), long.Parse(lastcommand.Split('-')[1]));
                else if (UserRepository.UserExists(long.Parse(User)))
                    ExamAccessRepository.Insert(long.Parse(User), long.Parse(lastcommand.Split('-')[1]));
                else
                    bot.SendTextMessageAsync(chatId, "موردی یافت نشد");
                bot.SendTextMessageAsync(chatId, "اختصاص داده شد", ParseMode.Default, false, false, 0, Keyboards.ExamEditor);
            }
            catch
            {
                bot.SendTextMessageAsync(chatId, "خظا در اختصاص دادن", ParseMode.Default, false, false, 0, Keyboards.ExamEditor);
            }
        }
        private StringBuilder UsersList()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\U0001F465 " + "لیست کاربران : " + Environment.NewLine);
            DataTable Users = UserRepository.SelectAll();
            foreach (DataRow row in Users.Rows)
            {
                stringBuilder.AppendLine("چت آیدی : [" + row[0].ToString() + "](tg://user?id=" + row[0].ToString() + ")" + Environment.NewLine + "نام : " + row[1].ToString());
                if (row[2].ToString() == "")
                {
                    stringBuilder.AppendLine("وضعیت ثبت نام : " + "تایید نشده");
                }
                else if (row[2].ToString() == "Verified")
                {
                    stringBuilder.AppendLine("وضعیت ثبت نام : " + "تایید شده");
                }
                else if (row[2].ToString() == "Blocked")
                {
                    stringBuilder.AppendLine("وضعیت ثبت نام : " + "بلاک شده");
                }
                else if (row[2].ToString().StartsWith("Management"))
                {
                    stringBuilder.AppendLine("وضعیت ثبت نام : " + "مدیریت");
                }
                stringBuilder.AppendLine("تاریخ ثبت نام : " + row[3].ToString());
                stringBuilder.AppendLine("شماره تلفن : " + row[4].ToString());
                stringBuilder.AppendLine("موجودی : " + row[6].ToString() + " تومان" + Environment.NewLine);
            }
            return stringBuilder;
        }
        private void updatenow()
        {
            try
            {
                BackUp();
                string path = Environment.CurrentDirectory + "\\Database.accdb";
                File.Delete(path);
                string url = "http://quiz24exam.parsaspace.com//Bot//Database.accdb";
                Uri uri = new Uri(url);
                WebClient webClient = new WebClient();
                webClient.DownloadFile(url, path);
                DataTable dt = SettingsRepository.SelectSettings();
                stopbot = (bool)dt.Rows[0][1];
                pused = (bool)dt.Rows[0][2];
                QuestionByQuestion = (bool)dt.Rows[0][3];
                AnswerSheet = (bool)dt.Rows[0][4];
                Fileid = dt.Rows[0][5].ToString();
                AutoBackUp = (bool)dt.Rows[0][6];
            }
            catch { }
        }
        private static void BackUp()
        {
            try
            {
                string path = Environment.CurrentDirectory + "\\" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + " Database.accdb";
                File.Copy(Environment.CurrentDirectory + "\\Database.accdb", path);
                var client = new RestClient("http://api.parsaspace.com/v1/files/upload");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6Im1vdXNpYW5pMTM4M0BnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiNTA5OTIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3ZlcnNpb24iOiIyIiwiaXNzIjoiaHR0cDovL2FwaS5wYXJzYXNwYWNlLmNvbS8iLCJhdWQiOiJBbnkiLCJleHAiOjE2NDE4MjM0OTAsIm5iZiI6MTYxMDI4NzQ5MH0.uymAXmDRqrc60FupQAwzQhMdUMTdE4_oa1qhmNt7Du8");
                request.AddHeader("content-type", "multipart/form-data");
                request.AddParameter("Domain", "quiz24exam.parsaspace.com");
                request.AddParameter("path", "/Bot/");
                request.AddFile("file", path);
                IRestResponse response = client.Execute(request);
                File.Delete(path);
            }
            catch { }
        }
        private void SendAS(string Ecode, string User, long chatId)
        {
            try
            {
                bot.SendTextMessageAsync(chatId, "در حال ارسال ...", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                adder("/start", chatId);
                DataTable Exam = ExamsRepository.SelectRow(long.Parse(Ecode));
                StringBuilder stringBuilder = new StringBuilder();
                if (User == "All")
                {
                    stringBuilder.AppendLine("آزمون " + Exam.Rows[0][1] + " با کد " + Ecode + " به همه کاربران با آیدی های زیر ارسال شد");
                    DataTable Users = UserRepository.SelectAll();
                    foreach (DataRow row in Users.Rows)
                    {
                        answSender(Ecode, long.Parse(row[0].ToString()), "همه کاربران", Exam.Rows[0][1].ToString());
                        stringBuilder.AppendLine("[" + row[0] + "](tg://user?id=" + row[0] + ")");
                    }
                }
                else if (User == "Verified")
                {
                    stringBuilder.AppendLine("آزمون " + Exam.Rows[0][1] + " با کد " + Ecode + " به کاربران تایید شده با آیدی های زیر ارسال شد");
                    DataTable Users = UserRepository.SelectByPanel("Verified");
                    foreach (DataRow row in Users.Rows)
                    {
                        answSender(Ecode, long.Parse(row[0].ToString()), "کاربران تایید شده", Exam.Rows[0][1].ToString());
                        stringBuilder.AppendLine("[" + row[0] + "](tg://user?id=" + row[0] + ")");
                    }
                }
                else if (User == "Blocked")
                {
                    stringBuilder.AppendLine("آزمون " + Exam.Rows[0][1] + " با کد " + Ecode + " به کاربران بلاک شده با آیدی های زیر ارسال شد");
                    DataTable Users = UserRepository.SelectByPanel("Blocked");
                    foreach (DataRow row in Users.Rows)
                    {
                        answSender(Ecode, long.Parse(row[0].ToString()), "کاربران بلاک شده", Exam.Rows[0][1].ToString());
                        stringBuilder.AppendLine("[" + row[0] + "](tg://user?id=" + row[0] + ")");
                    }
                }
                else if (User == "Nothing")
                {
                    stringBuilder.AppendLine("آزمون " + Exam.Rows[0][1] + " با کد " + Ecode + " به کاربران تایید نشده با آیدی های زیر ارسال شد");
                    DataTable Users = UserRepository.SelectByPanel("");
                    foreach (DataRow row in Users.Rows)
                    {
                        answSender(Ecode, long.Parse(row[0].ToString()), "کاربران تایید نشده", Exam.Rows[0][1].ToString());
                        stringBuilder.AppendLine("[" + row[0] + "](tg://user?id=" + row[0] + ")");
                    }
                }
                else
                {
                    stringBuilder.AppendLine("آزمون " + Exam.Rows[0][1] + " با کد " + Ecode + " به کاربر " + "[" + User + "](tg://user?id=" + User + ")" + " ارسال شد");
                    answSender(Ecode, long.Parse(User), "شخصی", Exam.Rows[0][1].ToString());
                }
                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2);
                bot.SendTextMessageAsync(ChatId_Channel, stringBuilder.ToString(), ParseMode.MarkdownV2);
            }
            catch { }
        }
        private void sendQBQ(string Ecode, string User, long chatId)
        {
            try
            {
                bot.SendTextMessageAsync(chatId, "در حال ارسال ...", ParseMode.Default, false, false, 0, Keyboards.ManagementKeyboard);
                adder("/start", chatId);
                DataTable Exam = ExamsRepository.SelectRow(long.Parse(Ecode));
                StringBuilder stringBuilder = new StringBuilder();
                if (User == "All")
                {
                    stringBuilder.AppendLine("آزمون " + Exam.Rows[0][1] + " با کد " + Ecode + " به همه کاربران با آیدی های زیر ارسال شد");
                    DataTable Users = UserRepository.SelectAll();
                    foreach (DataRow row in Users.Rows)
                    {
                        quesSender(Ecode, long.Parse(row[0].ToString()), "همه کاربران", Exam.Rows[0][1].ToString());
                        stringBuilder.AppendLine("[" + row[0] + "](tg://user?id=" + row[0] + ")");
                    }
                }
                else if (User == "Verified")
                {
                    stringBuilder.AppendLine("آزمون " + Exam.Rows[0][1] + " با کد " + Ecode + " به کاربران تایید شده با آیدی های زیر ارسال شد");
                    DataTable Users = UserRepository.SelectByPanel("Verified");
                    foreach (DataRow row in Users.Rows)
                    {
                        quesSender(Ecode, long.Parse(row[0].ToString()), "کاربران تایید شده", Exam.Rows[0][1].ToString());
                        stringBuilder.AppendLine("[" + row[0] + "](tg://user?id=" + row[0] + ")");
                    }
                }
                else if (User == "Blocked")
                {
                    stringBuilder.AppendLine("آزمون " + Exam.Rows[0][1] + " با کد " + Ecode + " به کاربران بلاک شده با آیدی های زیر ارسال شد");
                    DataTable Users = UserRepository.SelectByPanel("Blocked");
                    foreach (DataRow row in Users.Rows)
                    {
                        quesSender(Ecode, long.Parse(row[0].ToString()), "کاربران بلاک شده", Exam.Rows[0][1].ToString());
                        stringBuilder.AppendLine("[" + row[0] + "](tg://user?id=" + row[0] + ")");
                    }
                }
                else if (User == "Nothing")
                {
                    stringBuilder.AppendLine("آزمون " + Exam.Rows[0][1] + " با کد " + Ecode + " به کاربران تایید نشده با آیدی های زیر ارسال شد");
                    DataTable Users = UserRepository.SelectByPanel("");
                    foreach (DataRow row in Users.Rows)
                    {
                        quesSender(Ecode, long.Parse(row[0].ToString()), "کاربران تایید نشده", Exam.Rows[0][1].ToString());
                        stringBuilder.AppendLine("[" + row[0] + "](tg://user?id=" + row[0] + ")");
                    }
                }
                else
                {
                    stringBuilder.AppendLine("آزمون " + Exam.Rows[0][1] + " با کد " + Ecode + " به کاربر " + "[" + User + "](tg://user?id=" + User + ")" + " ارسال شد");
                    quesSender(Ecode, long.Parse(User), "شخصی", Exam.Rows[0][1].ToString());
                }
                bot.SendTextMessageAsync(chatId, stringBuilder.ToString(), ParseMode.MarkdownV2);
                bot.SendTextMessageAsync(ChatId_Channel, stringBuilder.ToString(), ParseMode.MarkdownV2);
            }
            catch { }
        }
        private void answSender(string code, long chatId, string usertype, string examname)
        {
            try
            {
                var one = bot.SendTextMessageAsync(chatId, "در حال ارسال پاسخنامه متنی توسط ادمین ...").Result.MessageId;
                DeleteRepository.Insert(long.Parse(code), chatId, one);
                StringBuilder answersheet = new StringBuilder();
                answersheet.AppendLine("\U0001F4CB " + "نام آزمون : " + examname + Environment.NewLine);
                foreach (DataRow row in QuestionsRepository.SelectByExamCode(long.Parse(code)).Rows)
                {
                    answersheet.AppendLine("گزینه " + int.Parse(row[2].ToString()).ToString("D2") + "  \U00002B05  " + row[5] + Environment.NewLine);
                }
                var two = bot.SendTextMessageAsync(chatId, answersheet.ToString());
                DeleteRepository.Insert(long.Parse(code), chatId, two.Result.MessageId);
                DownloadRepository.Insert(chatId, code, "پاسخنامه متنی", "ادمین", usertype, DateTime.Now, 0);
                Thread.Sleep(2000);
                bot.SendTextMessageAsync(chatId, "سوالات با موفقیت ارسال شد");
                removeTime = default;
                foreach (DataRow row in DeleteRepository.SelectDate().Rows)
                    removeTime = Convert.ToDateTime(row[0]);
            }
            catch { }
        }
        private void quesSender(string code, long chatId, string usertype, string examname)
        {
            try
            {
                var one = bot.SendTextMessageAsync(chatId, "در حال ارسال سوالات به صورت سوال به سوال توسط ادمین ...").Result.MessageId;
                DeleteRepository.Insert(long.Parse(code), chatId, one);
                var two = bot.SendTextMessageAsync(chatId, "\U0001F4CB " + "نام آزمون : " + examname).Result;
                DeleteRepository.Insert(long.Parse(code), chatId, two.MessageId);
                foreach (DataRow row in QuestionsRepository.SelectByExamCode(long.Parse(code)).Rows)
                {
                    Thread.Sleep(1000);
                    imagesender(long.Parse(code), chatId, row[2].ToString(), row[6].ToString(), row[5].ToString(), row[3].ToString());
                }
                DownloadRepository.Insert(chatId, code, "سوال به سوال", "ادمین", usertype, DateTime.Now, 0);
                Thread.Sleep(2000);
                bot.SendTextMessageAsync(chatId, "سوالات با موفقیت ارسال شد");
                removeTime = default;
                foreach (DataRow row in DeleteRepository.SelectDate().Rows)
                    removeTime = Convert.ToDateTime(row[0]);
            }
            catch { }
        }
        private void answ(string code, long chatId, long Amount)
        {
            try
            {
                string examname = ExamsRepository.SelectRow(long.Parse(code)).Rows[0][1].ToString();
                var messagetext = "\U0001F4E5 " + "آزمون " + examname.Replace('-', ' ') + " به صورت پاسخنامه متنی توسط [" + chatId + "](tg://user?id=" + chatId + ") دریافت شد" + " \U00002705";
                bot.SendTextMessageAsync(CahtId_Admin, messagetext.Replace("  ", " "), ParseMode.MarkdownV2);
                bot.SendTextMessageAsync(ChatId_Channel, messagetext.Replace("  ", " "), ParseMode.MarkdownV2);
                var one = bot.SendTextMessageAsync(chatId, "در حال ارسال ...").Result.MessageId;
                DeleteRepository.Insert(long.Parse(code), chatId, one);
                Thread.Sleep(1000);
                StringBuilder answersheet = new StringBuilder();
                answersheet.AppendLine("\U0001F4CB " + "نام آزمون : " + examname + Environment.NewLine);
                foreach (DataRow row in QuestionsRepository.SelectByExamCode(long.Parse(code)).Rows)
                {
                    answersheet.AppendLine("گزینه " + int.Parse(row[2].ToString()).ToString("D2") + "  \U00002B05  " + row[5] + Environment.NewLine);
                }
                var two = bot.SendTextMessageAsync(chatId, answersheet.ToString()).Result;
                DeleteRepository.Insert(long.Parse(code), chatId, two.MessageId);
                DownloadRepository.Insert(chatId, code, "پاسخنامه متنی", "کاربر", "شخصی", DateTime.Now, Amount);
                UserRepository.UpdateAmount(chatId, Amount * -1);
                Thread.Sleep(2000);
                bot.SendTextMessageAsync(chatId, "سوالات با موفقیت ارسال شد", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                adder("/start", chatId);
                removeTime = default;
                foreach (DataRow row in DeleteRepository.SelectDate().Rows)
                    removeTime = Convert.ToDateTime(row[0]);
            }
            catch { }
        }
        private void ques(string code, long chatId, long Amount)
        {
            try
            {
                string examname = ExamsRepository.SelectRow(long.Parse(code)).Rows[0][1].ToString();
                var messagetext = "\U0001F4E5 " + "آزمون " + examname.Replace('-', ' ') + " به صورت سوال به سوال توسط [" + chatId + "](tg://user?id=" + chatId + ") دریافت شد" + " \U00002705";
                bot.SendTextMessageAsync(CahtId_Admin, messagetext.Replace("  ", " "), ParseMode.MarkdownV2);
                bot.SendTextMessageAsync(ChatId_Channel, messagetext.Replace("  ", " "), ParseMode.MarkdownV2);
                var one = bot.SendTextMessageAsync(chatId, "در حال ارسال ...").Result.MessageId;
                DeleteRepository.Insert(long.Parse(code), chatId, one);
                Thread.Sleep(1000);
                var two = bot.SendTextMessageAsync(chatId, "\U0001F4CB " + "نام آزمون : " + examname).Result;
                DeleteRepository.Insert(long.Parse(code), chatId, two.MessageId);
                foreach (DataRow row in QuestionsRepository.SelectByExamCode(long.Parse(code)).Rows)
                {
                    Thread.Sleep(1000);
                    imagesender(long.Parse(code), chatId, row[2].ToString(), row[6].ToString(), row[5].ToString(), row[3].ToString());
                }
                DownloadRepository.Insert(chatId, code, "سوال به سوال", "کاربر", "شخصی", DateTime.Now, Amount);
                UserRepository.UpdateAmount(chatId, Amount * -1);
                Thread.Sleep(2000);
                bot.SendTextMessageAsync(chatId, "سوالات با موفقیت ارسال شد", ParseMode.Default, false, false, 0, Keyboards.VerifiedKeyboardMarkup);
                adder("/start", chatId);
                removeTime = default;
                foreach (DataRow row in DeleteRepository.SelectDate().Rows)
                    removeTime = Convert.ToDateTime(row[0]);
            }
            catch { }
        }
        private string StatusUpdate()
        {
            string txt = "\U0001F5BC تصویر به تصویر";
            if (QuestionByQuestion)
                txt += " \U00002705";
            else
                txt += " \U0000274C";
            txt += Environment.NewLine + "\U0001F4C4 پاسخنامه متنی";
            if (AnswerSheet)
                txt += " \U00002705";
            else
                txt += " \U0000274C";
            return txt;
        }
        private async void imagesender(long ExamCode, long ChatId, string ID, string HardLevel, string Key, string QuestionImage)
        {
            try
            {
                //"\U0001F194" + "شماره سوال : " + ID + Environment.NewLine + 
                InputOnlineFile inputOnlineFile = new InputOnlineFile(QuestionImage);
                var message = await bot.SendPhotoAsync(ChatId, inputOnlineFile, "\U0001F39A" + "درجه سختی : " + HardLevel + Environment.NewLine + "\U0001F5DD" + "کلید : " + Key);
                DeleteRepository.Insert(ExamCode, ChatId, message.MessageId);
            }
            catch { }
        }
        private async void sendfile(Telegram.Bot.Types.Update up)
        {
            try
            {
                InputOnlineFile inputOnlineFile = new InputOnlineFile(Fileid);
                await bot.SendDocumentAsync(up.Message.Chat.Id, inputOnlineFile, "نرم افزار Quiz برای ویندوز" + " \U0001f929" + Environment.NewLine + "با قابلیت های جدید" + " \U0001F60D");
            }
            catch { }
        }
        private void adder(string text, long chatId)
        {
            try
            {
                UserRepository.Updatelastcommand(chatId, text);
            }
            catch { }
        }
        private void Bot_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (botThread != null)
                    botThread.Abort();
                BackUp();
            }
            catch { }
        }
        private void FormHandler_Formclose()
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => { FormHandler_Formclose(); }));
            }
            else
            {
                Close();
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (AutoBackUp)
                    if (lefttime > 0)
                        lefttime--;
                    else
                    {
                        BackUp();
                        lefttime = MainTime;
                    }
            }
            catch { }
        }
        private void questions_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!pused)
                {
                    if (removeTime <= DateTime.Now && removeTime.ToString() != "1/1/0001 12:00:00 AM")
                    {
                        foreach (DataRow row in DeleteRepository.SelectByDate(removeTime).Rows)
                        {
                            messagedelete(long.Parse(row[0].ToString()), int.Parse(row[1].ToString()));
                        }
                        DeleteRepository.RemoveByDate(removeTime);
                        bot.SendTextMessageAsync(CahtId_Admin, "سوالات به صورت خودکار با موفقیت حذف شد" + " \U00002705");
                        bot.SendTextMessageAsync(ChatId_Channel, "سوالات به صورت خودکار با موفقیت حذف شد" + " \U00002705");
                        removeTime = default;
                        foreach (DataRow row in DeleteRepository.SelectDate().Rows)
                            removeTime = Convert.ToDateTime(row[0]);
                    }
                }
            }
            catch { }
        }
        private async void messagedelete(long ChatId, int MessageId)
        {
            try
            {
                await bot.DeleteMessageAsync(ChatId, MessageId);
            }
            catch { }
        }
        private List<string> uploades()
        {
            List<string> list = new List<string>();
            try
            {
                Uri uri = new Uri("http://quiz24exam.parsaspace.com/");
                var client = new RestClient("http://api.parsaspace.com/v1/files/list");
                var request = new RestRequest(Method.POST);
                request.AddHeader("authorization", "Bearer " + "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1bmlxdWVfbmFtZSI6Im1vdXNpYW5pMTM4M0BnbWFpbC5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiNTA5OTIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3ZlcnNpb24iOiIyIiwiaXNzIjoiaHR0cDovL2FwaS5wYXJzYXNwYWNlLmNvbS8iLCJhdWQiOiJBbnkiLCJleHAiOjE2NDE4MjM0OTAsIm5iZiI6MTYxMDI4NzQ5MH0.uymAXmDRqrc60FupQAwzQhMdUMTdE4_oa1qhmNt7Du8");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("Domain", uri.Host);
                request.AddParameter("path", "/Exams/");
                IRestResponse response = client.Execute(request);
                var json = JsonConvert.DeserializeObject(response.Content).ToString();
                StringReader stringr = new StringReader(json);
                string line = stringr.ReadLine(); ;
                while (line != null)
                {
                    if (line.Contains("Name"))
                    {
                        list.Add(line.Replace('"', ' ').Replace(',', ' ').Replace("Name :  ", string.Empty).Replace(".txt", string.Empty).Trim().ToString());
                    }
                    line = stringr.ReadLine();
                }
                stringr.Close();
            }
            catch { }
            return list;
        }
    }
}