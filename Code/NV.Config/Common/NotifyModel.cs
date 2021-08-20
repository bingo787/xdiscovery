using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NV.Config
{
    /// <summary>
    /// ID:属性改变通知类
    /// Describe:属性改变激发通知两个方法
    /// Author:ybx
    /// Date:2016年11月22日15:35:29
    /// </summary>
    public class NotifyModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性改变通知：表达式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        public void RaisePropertyChanged<T>(Expression<Func<T>> func)
        {
            if (func == null)
                throw new InvalidOperationException("错误的属性表达式！");
            MemberExpression mep = func.Body as MemberExpression;
            if (mep == null)
                throw new InvalidOperationException("错误的属性表达式！");
            PropertyInfo pro = mep.Member as PropertyInfo;
            if (pro == null)
                throw new InvalidOperationException("错误的属性表达式！");
            string proName = pro.Name;
            RaisePropertyChanged(proName);
        }
        /// <summary>
        /// 针对ViewModel的属性Set方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public T Set<T>(Expression<Func<T>> func, ref T property, T value)
        {
            property = value;
            RaisePropertyChanged(func);
            return property;
        }
        /// <summary>
        /// 属性改变通知:属性名
        /// </summary>
        /// <param name="name"></param>
        public void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
