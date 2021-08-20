using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NV.DRF.Core.Global;
using NV.DRF.Core.Interface;
using NV.DRF.Core.Model;

namespace ExamModule.Service
{
    public class MenuGroupService : IMenuGroupService
    {
        #region IMenuGroupService 成员

        public ObservableCollection<MenuGroup> GetMenuGroup()
        {
            ObservableCollection<MenuGroup> group = new ObservableCollection<MenuGroup>();
            MenuGroup menu = new MenuGroup()
            {
                Header = "自动化测试",
                Describe = "探测器自动化测试",
                IcoSource = new Uri("pack://application:,,,/NV.DRF.Controls;Component/themes/images/btn_exam.png", UriKind.Absolute),
                //ViewType = typeof(ExamModule.Views.ExamDetector),
                SortFlag = 2,
                IsEnabled = false,
                ModuleModeMapping = ModuleMode.Exam,
            };
            group.Add(menu);
            return group;
        }

        #endregion

    }
}
