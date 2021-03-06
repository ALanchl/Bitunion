﻿using Bitunion.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Bitunion
{
    public partial class MainPage : PhoneApplicationPage
    {
        //主页VM
        private MainViewModel _mainvm= new MainViewModel();

        //父子论坛视图对象
        public static Dictionary<string,List<BuForum>> DictFourm = new Dictionary<string,List<BuForum>>();

        //本地登录数据以及配置信息
        private IsolatedStorageSettings userlogininfo = IsolatedStorageSettings.ApplicationSettings;

        //加载论坛列表的锁
        bool isloadingforumlist = false;
        
        // 构造函数
        public MainPage()
        {
            InitializeComponent();
            DataContext = _mainvm;
           
            DictFourm = new Dictionary<string,List<BuForum>>();
            pgBar.Visibility = Visibility.Collapsed;
        }

        // 为 ViewModel 项加载数据
        protected override  void OnNavigatedTo(NavigationEventArgs e)
        {
            //消除后退堆栈中的登录页面
            NavigationService.RemoveBackEntry();
            LoadLatestThreadList();
        }                                                                                                                                                                                                 

        //响应在最新帖子列表中选择某帖子的事件
        private void LongListSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            // If selected item is null (no selection) do nothing
            if (LatestThreadViewList.SelectedItem == null)
                return;

            ThreadViewModel item = LatestThreadViewList.SelectedItem as ThreadViewModel;

            LatestThreadViewList.SelectedItem = null;

            var thread  = item.latestthread;
            
            // Navigate to the new page
            NavigationService.Navigate(new Uri("/BuThreadPage.xaml?tid=" + thread.tid
                + "&subject="+ thread.pname 
                + "&replies=" + thread.tid_sum
                + "&fid=" + thread.fid
                + "&fname=" + thread.fname
                , UriKind.Relative));
        }
        
        //异步加载最新帖子列表
        private async void LoadLatestThreadList()
        {
            if (_mainvm.LatestThreadItems.Count != 0)
                return;

            pgBar.Visibility = Visibility.Visible;
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = false;
            List<BuLatestThread> btl = await BuAPI.QueryLatestThreadList();
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = true;
            pgBar.Visibility = Visibility.Collapsed;

            if (btl == null)
                return;

            foreach (BuLatestThread bt in btl)
                _mainvm.LatestThreadItems.Add(new ThreadViewModel(bt));

        }
        
        //异步加载论坛列表
        private async void LoadForumList()
        {
            if (_mainvm.ForumItems.Count() != 0)
                return;

            if (isloadingforumlist)
                return;

            isloadingforumlist = true;

            pgBar.Visibility = Visibility.Visible;
          List<BuGroupForum> bl = await BuAPI.QueryForumList();
          pgBar.Visibility = Visibility.Collapsed;
          if (bl == null)
          {
              isloadingforumlist = false;
              return;
          }

          foreach (BuGroupForum bt in bl)
          {
              if (bt.main == null)
                  continue;
              if(bt.main.Count != 1)
                  continue;

              //添加主论坛
              _mainvm.ForumItems.Add(new ForumViewModel(bt.main[0]));

              if (bt.sub == null)
                  continue;

              DictFourm[bt.main[0].fid] = new List<BuForum>();
              foreach (var subforum in bt.sub)
                  DictFourm[bt.main[0].fid].Add(subforum);
          }
            
            isloadingforumlist = false;
        }
        
        //刷新最新的帖子列表
        private void refresh ()
        {
            _mainvm.LatestThreadItems.Clear();
            LoadLatestThreadList();
        }
        
        //点击刷新按钮
         private void refresh_click(object sender, EventArgs e)
         {
             refresh();
         }

        //切换Tab页面
         private void MainPagePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
         {
             //如果切换到论坛分组的页面才加载论坛分组列表
             if (MainPagePivot.SelectedIndex == 1)
                    LoadForumList();
         }

         //响应论坛列表选择进入某一个论坛的事件
         private void ForumViewList_SelectionChanged(object sender, SelectionChangedEventArgs e)
         {
                if (ForumViewList.SelectedItem == null)
                    return;

                 ForumViewModel item = ForumViewList.SelectedItem as ForumViewModel;

                ForumViewList.SelectedItem = null;

              var forum = item.forum;

              //Navigate to the new page
                NavigationService.Navigate(new Uri("/BuForumPage.xaml?fid=" + forum.fid
                    + "&fname=" + forum.name
                   , UriKind.Relative));
         }

         private void logout_click(object sender, EventArgs e)
         {
             NavigationService.Navigate(new Uri("/LoginPage.xaml?type=logout", UriKind.Relative));
         }

         private void Setting_Click(object sender, EventArgs e)
         {
             NavigationService.Navigate(new Uri("/BuSettingPage.xaml", UriKind.Relative));
         }

        // 用于生成本地化 ApplicationBar 的示例代码
        //private void BuildLocalizedApplicationBar()
        //{
        //    // 将页面的 ApplicationBar 设置为 ApplicationBar 的新实例。
        //    ApplicationBar = new ApplicationBar();

        //    // 创建新按钮并将文本值设置为 AppResources 中的本地化字符串。
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // 使用 AppResources 中的本地化字符串创建新菜单项。
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}
