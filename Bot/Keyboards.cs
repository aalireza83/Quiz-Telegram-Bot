using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot
{
    class Keyboards
    {
        public ReplyKeyboardMarkup ManagementKeyboard, KeyboardSend, ExamManagement, ExamEditor, QuestionManagement, AccessManagement, MessagesManagement, PayManagement, TransactionsManagement, KeyboardUsersType, BotSettings, SignupKeyboardMarkup, signupedKeyboardMarkup, finishKeyboardMarkup, VerifiedKeyboardMarkup, BlockedKeyboardMarkup, BackKeyboardMarkup, BackMainKeyboardMarkup;
        public Keyboards()
        {
            #region Management
            ManagementKeyboard = new ReplyKeyboardMarkup();
            ManagementKeyboard.ResizeKeyboard = true;
            KeyboardButton[] Managementrow1 =
            {
                   new KeyboardButton("مدیریت دانلود ها" + " \U0001F4E5"),new KeyboardButton("مدیریت آزمون ها" + " \U0001F4DA")
            };
            KeyboardButton[] Managementrow2 =
            {
                    new KeyboardButton("مدیریت پیام ها" + " \U00002709"),new KeyboardButton("مدیریت کاربران" + " \U0001F465")
            };
            KeyboardButton[] Managementrow3 =
            {
                   new KeyboardButton("مدیریت تراکنش ها" + " \U0001F9FE") , new KeyboardButton("مدیریت پرداخت ها" + " \U0001F4B3")
            };
            KeyboardButton[] Managementrow4 =
            {
                   new KeyboardButton("مدیریت دسترسی ها" + " \U0001F3F7") ,new KeyboardButton("تنظیمات ربات" + " \U00002699")
            };
            KeyboardButton[] Managementrow5 =
            {
                   new KeyboardButton("خروج از پنل کاربری" + " \U0000274C")
            };
            ManagementKeyboard.Keyboard = new KeyboardButton[][]
            {
                   Managementrow1,Managementrow2,Managementrow3,Managementrow4,Managementrow5
            };
            #endregion
            #region KeyboardSend
            KeyboardSend = new ReplyKeyboardMarkup();
            KeyboardSend.ResizeKeyboard = true;
            KeyboardButton[] KeyboardSendrow1 =
            {
                                  new KeyboardButton("ارسال پاسخنامه متنی" + " \U0001F4CB"), new KeyboardButton("ارسال سوال به سوال" + " \U0001F5BC")
                            };
            KeyboardButton[] KeyboardSendrow2 =
            {
                                 new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
                            };
            KeyboardSend.Keyboard = new KeyboardButton[][]
                {
                            KeyboardSendrow1,KeyboardSendrow2
                }; 
            #endregion
            #region ExamManagement
            ExamManagement = new ReplyKeyboardMarkup();
            ExamManagement.ResizeKeyboard = true;
            KeyboardButton[] ExamManagementrow1 =
            {
                   new KeyboardButton("همه آزمون ها" + " \U0001F4DA")  , new KeyboardButton("آپدیت" + " \U0001F504")
            };
            KeyboardButton[] ExamManagementrow2 =
            {
                   new KeyboardButton("بازگشت" + " \U0001F519")
            };
            ExamManagement.Keyboard = new KeyboardButton[][]
            {
                  ExamManagementrow1,ExamManagementrow2
            };
            #endregion
            #region ExamEditor
            ExamEditor = new ReplyKeyboardMarkup();
            ExamEditor.ResizeKeyboard = true;
            KeyboardButton[] ExamEditorrow1 =
            {
                   new KeyboardButton("دریافت سوالات" + " \U0001F4E5")  , new KeyboardButton("ارسال آزمون" + " \U0001F4E4")
            };
            KeyboardButton[] ExamEditorrow2 =
            {
                   new KeyboardButton("افزودن دسترسی" + " \U0001F3F7"),new KeyboardButton("تعیین زمان پایان" + " \U000023F2")
            };
            KeyboardButton[] ExamEditorrow3 =
            {
                   new KeyboardButton("حذف آزمون" + " \U0001F5D1")
            };
            KeyboardButton[] ExamEditorrow4 =
            {
                   new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
            };
            ExamEditor.Keyboard = new KeyboardButton[][]
            {
                  ExamEditorrow1,ExamEditorrow2,ExamEditorrow3,ExamEditorrow4
            };
            #endregion
            #region QuestionManagement
            QuestionManagement = new ReplyKeyboardMarkup();
            QuestionManagement.ResizeKeyboard = true;
            KeyboardButton[] Questionmanagementrow1 =
            {
                    new KeyboardButton("تنظیمات دانلود سوالات" + " \U00002699"),new KeyboardButton("حذف سوالات" + " \U0001F5D1")
            };
            KeyboardButton[] Questionmanagementrow2 =
            {
                   new KeyboardButton("همه دانلود ها" + " \U0001F4E5"),new KeyboardButton("جست و جو بر اساس چت آیدی" + " \U0001F50D")
            };
            KeyboardButton[] Questionmanagementrow3 =
            {
                   new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("جست و جو بر اساس آزمون" + " \U0001F4DA")
            };
            QuestionManagement.Keyboard = new KeyboardButton[][]
            {
                   Questionmanagementrow1,Questionmanagementrow2,Questionmanagementrow3
            };
            #endregion
            #region AccessManagement
            AccessManagement = new ReplyKeyboardMarkup();
            AccessManagement.ResizeKeyboard = true;
            KeyboardButton[] AccessManagementrow1 =
            {
                   new KeyboardButton("حذف دسترسی" + " \U0001F5D1"),new KeyboardButton("جست و جو بر اساس آزمون" + " \U0001F4DA")
            };
            KeyboardButton[] AccessManagementrow2 =
            {
                   new KeyboardButton("همه دسترسی ها" + " \U0001F4E5"),new KeyboardButton("جست و جو بر اساس چت آیدی" + " \U0001F50D")
            };
            KeyboardButton[] AccessManagementrow3 =
            {
                   new KeyboardButton("بازگشت" + " \U0001F519")
            };
            AccessManagement.Keyboard = new KeyboardButton[][]
            {
                   AccessManagementrow1,AccessManagementrow2,AccessManagementrow3
            };
            #endregion
            #region MessagesManagement
            MessagesManagement = new ReplyKeyboardMarkup();
            MessagesManagement.ResizeKeyboard = true;
            KeyboardButton[] Messagesmanagementrow1 =
            {
                   new KeyboardButton("همه پیام ها" + " \U00002709"),new KeyboardButton("ارسال پیام" + "\U0001F4DD" )
            };
            KeyboardButton[] Messagesmanagementrow2 =
            {
                   new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("جست و جو بر اساس چت آیدی" + " \U0001F50D")
            };
            MessagesManagement.Keyboard = new KeyboardButton[][]
            {
                   Messagesmanagementrow1,Messagesmanagementrow2
            };
            #endregion
            #region PayManagement
            PayManagement = new ReplyKeyboardMarkup();
            PayManagement.ResizeKeyboard = true;
            KeyboardButton[] PayManagementrow1 =
            {
                   new KeyboardButton("حذف رسید پرداخت" + " \U0001F5D1"),new KeyboardButton("افزودن رسید پرداخت" + " \U00002795")
            };
            KeyboardButton[] PayManagementrow2 =
            {
                   new KeyboardButton("جست و جو بر اساس چت آیدی" + " \U0001F50D"), new KeyboardButton("همه پرداخت ها" + " \U0001F4B3")
            };
            KeyboardButton[] PayManagementrow3 =
            {
                   new KeyboardButton("بازگشت" + " \U0001F519")
            };
            PayManagement.Keyboard = new KeyboardButton[][]
            {
                   PayManagementrow1,PayManagementrow2,PayManagementrow3
            };
            #endregion
            #region TransactionsManagement
            TransactionsManagement = new ReplyKeyboardMarkup();
            TransactionsManagement.ResizeKeyboard = true;
            KeyboardButton[] TransactionsManagementrow1 =
            {
                   new KeyboardButton("حذف تراکنش" + " \U0001F5D1"),new KeyboardButton("افزودن تراکنش" + " \U00002795")
            };
            KeyboardButton[] TransactionsManagementrow2 =
            {
                   new KeyboardButton("جست و جو بر اساس چت آیدی" + " \U0001F50D"), new KeyboardButton("همه تراکنش ها" + " \U0001F9FE")
            };
            KeyboardButton[] TransactionsManagementrow3 =
            {
                   new KeyboardButton("بازگشت" + " \U0001F519")
            };
            TransactionsManagement.Keyboard = new KeyboardButton[][]
            {
                   TransactionsManagementrow1,TransactionsManagementrow2,TransactionsManagementrow3
            };
            #endregion
            #region KeyboardUsersType
            KeyboardUsersType = new ReplyKeyboardMarkup();
            KeyboardUsersType.ResizeKeyboard = true;
            KeyboardButton[] row1 =
            {
                                  new KeyboardButton("همه کاربران" + " \U0001F465"),new KeyboardButton("کاربران تایید شده" + " \U00002705")
                            };
            KeyboardButton[] row2 =
            {
                                  new KeyboardButton("کاربران بلاک شده" + " \U000026D4"),new KeyboardButton("کاربران تایید نشده" + " \U00002754")
                            };
            KeyboardButton[] row3 =
            {
                                  new KeyboardButton("بازگشت" + " \U0001F519"),
                            };
            KeyboardUsersType.Keyboard = new KeyboardButton[][]
                {
                            row1,row2,row3
                };
            #endregion
            #region BotSettings
            BotSettings = new ReplyKeyboardMarkup();
            BotSettings.ResizeKeyboard = true;
            KeyboardButton[] BotSettingsrow1 =
            {
                   new KeyboardButton("آپدیت" + " \U0001F5A5") ,new KeyboardButton("بکاپ" + " \U0001F4BE")
            };
            KeyboardButton[] BotSettingsrow2 =
            {
                   new KeyboardButton("متوقف کردن ربات" + " \U0001F512"),new KeyboardButton("راه اندازی ربات" + " \U0001F513")
            };
            KeyboardButton[] BotSettingsrow3 =
            {
                   new KeyboardButton("فایل نرم افزار" + " \U0001F4BB") ,new KeyboardButton("بکاپ گیری خودکار" + " \U0001F4BE")
            };
            KeyboardButton[] BotSettingsrow4 =
            {
                   new KeyboardButton("بازگشت" + " \U0001F519") , new KeyboardButton("بستن برنامه" + " \U0000274C")
            };
            BotSettings.Keyboard = new KeyboardButton[][]
            {
                   BotSettingsrow1,BotSettingsrow2,BotSettingsrow3,BotSettingsrow4
            };
            #endregion
            #region Signup
            SignupKeyboardMarkup = new ReplyKeyboardMarkup();
            SignupKeyboardMarkup.ResizeKeyboard = true;
            KeyboardButton[] Signuprow1 =
            {
                 new KeyboardButton("ارتباط با ما" + " \U0001F4AC"),new KeyboardButton("ثبت نام" + " \U0000270D")
            };
            SignupKeyboardMarkup.Keyboard = new KeyboardButton[][]
            {
                 Signuprow1
            };
            #endregion
            #region signuped
            signupedKeyboardMarkup = new ReplyKeyboardMarkup();
            signupedKeyboardMarkup.ResizeKeyboard = true;
            KeyboardButton button = KeyboardButton.WithRequestContact("\U0001F4F2 ارسال شماره تلفن");
            KeyboardButton[] signupedrow1 =
            {
                            new KeyboardButton("ارتباط با ما" + " \U0001F4AC"),button
            };
            signupedKeyboardMarkup.Keyboard = new KeyboardButton[][]
            {
                            signupedrow1
            };
            #endregion
            #region finish
            finishKeyboardMarkup = new ReplyKeyboardMarkup();
            finishKeyboardMarkup.ResizeKeyboard = true;
            KeyboardButton[] finishrow1 =
            {
                            new KeyboardButton("ارتباط با ما" + " \U0001F4AC")
                        };
            finishKeyboardMarkup.Keyboard = new KeyboardButton[][]
            {
                            finishrow1
            };
            #endregion
            #region Verified
            VerifiedKeyboardMarkup = new ReplyKeyboardMarkup();
            VerifiedKeyboardMarkup.ResizeKeyboard = true;
            KeyboardButton[] Verifiedrow1 =
            {
                                 new KeyboardButton("دریافت آزمون" + " \U0001F4E5")
            };
            KeyboardButton[] Verifiedrow2 =
            {
                                 new KeyboardButton("لیست دانلود ها" + " \U0001F4C4"),new KeyboardButton("لیست تراکنش ها" + " \U0001F9FE")
            };
            KeyboardButton[] Verifiedrow3 =
            {
                                 new KeyboardButton("ارتباط با ما" + " \U0001F4AC"),new KeyboardButton("مشخصات کاربری" + " \U0001F464")
            };
            VerifiedKeyboardMarkup.Keyboard = new KeyboardButton[][]
                {
                            Verifiedrow1,Verifiedrow2,Verifiedrow3
            };
            #endregion
            #region Blocked
            BlockedKeyboardMarkup = new ReplyKeyboardMarkup();
            BlockedKeyboardMarkup.ResizeKeyboard = true;
            KeyboardButton[] Blockedrow1 =
            {
                            new KeyboardButton("ارتباط با ما" + " \U0001F4AC")
                        };
            BlockedKeyboardMarkup.Keyboard = new KeyboardButton[][]
            {
                            Blockedrow1
            };
            #endregion
            #region Back
            BackKeyboardMarkup = new ReplyKeyboardMarkup();
            BackKeyboardMarkup.ResizeKeyboard = true;
            KeyboardButton[] Backrow1 =
            {
                            new KeyboardButton("بازگشت" + " \U0001F519")
                        };
            BackKeyboardMarkup.Keyboard = new KeyboardButton[][]
            {
                            Backrow1
            };
            #endregion
            #region Back&Main
            BackMainKeyboardMarkup = new ReplyKeyboardMarkup();
            BackMainKeyboardMarkup.ResizeKeyboard = true;
            KeyboardButton[] BackMainrow1 =
            {
                            new KeyboardButton("بازگشت" + " \U0001F519"),new KeyboardButton("صغحه اصلی" + " \U0001F3E0")
                        };
            BackMainKeyboardMarkup.Keyboard = new KeyboardButton[][]
            {
                            BackMainrow1
            };
            #endregion
        }
    }
}
