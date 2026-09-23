using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Sc2Switch2;

namespace Sc2Wpf;

public partial class SwissReferenceView:UserControl {
    readonly bool guide;
    readonly Func<string> errorTitle,errorDetail,gameLanguage;
    public event Action BackRequested;
    public event Action EditRequested;
    sealed record Article(string Number,string Title,string Body) {
        public Visibility NumberVisibility=>Number==null?Visibility.Collapsed:Visibility.Visible;
    }
    public SwissReferenceView(bool guide,Func<string> errorTitle,Func<string> errorDetail,Func<string> gameLanguage){
        InitializeComponent();this.guide=guide;this.errorTitle=errorTitle;this.errorDetail=errorDetail;this.gameLanguage=gameLanguage;
        Refresh();SizeChanged+=(_,_)=>AdaptLayout();
        Loaded+=(_,_)=>UiText.Instance.PropertyChanged+=LanguageChanged;
        Unloaded+=(_,_)=>UiText.Instance.PropertyChanged-=LanguageChanged;
    }
    void LanguageChanged(object sender,PropertyChangedEventArgs e)=>Refresh();
    void Refresh(){
        Category.Text=UiText.T(guide?"web.fieldGuide":"web.diagnostics");
        Heading.Text=UiText.T(guide?"web.helpTitle":"web.errorTitle");Heading.FontFamily=UiTypography.DisplayForLanguage(UiText.Language);
        Introduction.Text=guide?UiText.T("切换流程、使用条件及状态说明。"):errorTitle();
        ErrorPanel.Visibility=guide?Visibility.Collapsed:Visibility.Visible;ErrorContent.Text=errorDetail();
        EditButton.Content=UiText.T("web.openInstallationSettings");EditButton.Visibility=guide?Visibility.Collapsed:Visibility.Visible;
        BackButton.Content=UiText.T("返回切换");
        var articles=new List<Article>();
        void Add(string number,string title,string body)=>articles.Add(new(number,UiText.T(title),UiText.T(body)));
        if(guide){
            Add("01","1. 切换前提","星际争霸 II 和地图编辑器需处于关闭状态。战网下载或更新任务需已完成。");
            Add("02","2. 目标配置",UiText.Format("web.helpLanguage",gameLanguage()));
            Add("03","3. 登录与启动","切换过程会重新启动战网。账号登录及游戏启动在战网中完成。");
            Add("↳","状态含义","“战网当前配置”读取自本地配置文件，不表示账号已登录。“本地安装检查通过”不表示已完成账号认证或在线连接验证。");
            Add("↳","切换期间","切换期间会锁定目标和界面语言，并阻止关闭窗口。选错目标时，请等待结束后再切换。");
        }else{
            Add(null,"使用条件","游戏及地图编辑器需处于关闭状态。战网下载或更新完成后，可执行“重新检查”。");
            Add(null,"进一步检查","web.checkSettingsHint");
        }
        Articles.ItemsSource=articles;
    }
    void AdaptLayout(){
        double gutter=ActualWidth<=420?14:ActualWidth<=600?18:ActualWidth<=900?24:40;
        ReadingArea.Margin=new Thickness(gutter,24,gutter,24);ReferenceDock.Padding=new Thickness(gutter,14,gutter,14);Heading.FontSize=ActualWidth<=600?30:36;
        bool wrap=!guide&&ActualWidth<=600;
        Grid.SetRow(ReferenceActions,wrap?1:0);Grid.SetColumn(ReferenceActions,wrap?0:1);Grid.SetColumnSpan(ReferenceActions,wrap?2:1);
        ReferenceActions.Margin=new Thickness(0,wrap?8:0,0,0);EditButton.MaxWidth=Math.Max(120,ActualWidth*(wrap?.55:.35));
        ErrorContentFrame.Padding=ActualWidth<=480?new Thickness(18,16,18,16):new Thickness(24,20,24,20);
    }
    void Back_Click(object sender,RoutedEventArgs e)=>BackRequested?.Invoke();
    void Edit_Click(object sender,RoutedEventArgs e)=>EditRequested?.Invoke();
}
