using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace SmallStory.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : SmallStory.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        #region 数据操作函数
        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        } 
        #endregion

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");

            #region 第1章
            var group1 = new SampleDataGroup("Group-1",
            "第1章",
            "Group Subtitle: 1",
            "Assets/title/1.png",
            "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
"鱼竿和鱼",
"Item Subtitle: 3",
"Assets/title/1.png",
"Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
@"
      从前，有两个饥饿的人得到了一位长者的恩赐：一根鱼竿和一篓鲜活硕大的鱼。其中，一个人要了一篓鱼，另一个人要了一根鱼竿，于是他们分道扬镳了。得到鱼的人原地就用干柴搭起篝火煮起了鱼，他狼吞虎，咽，并没有品出鲜鱼的肉香，转瞬间，连鱼带汤就被他吃了个精光，鱼吃完了，他又陷入了饥饿中，不久，便饿死了。另一个人则提着鱼竿继续忍饥挨饿，一步步艰难地向大海边走去，他想赶到海边钓很多的鱼，这样以后就不会挨饿了。可当他已经看到不远处那处蔚蓝色的海洋时，他浑身的最后一点力气也使完了，只能眼巴巴地带?无尽的遗憾撒手人寰。
      又有两个饥饿的人，他们同样得到了长者恩赐的一根鱼竿和一篓鱼。他们没有像前面两人一样各奔东西，而是商定共同去找寻大海，钓更多的鱼，改变命运。于是他们出发了，他们总是在饥饿难耐的时候才煮一条鱼，经过遥远的跋涉，他们终于来到了海边，从此，两人开始了以捕鱼为生的日子。几后年，他们盖起了房子，有了各自的家庭、子女，有了自己建造的渔船，过上了幸福安康的生活。
      
【温馨提示】

      一个人只顾眼前的黎明前，得到的终将是短暂的欢愉；一个人目标高远，但也要面对现实的生活，即制订的目标在现实有可行性。只有把理想和现实有机结合起来，寻找最佳捷径，与他人团结合作才有可能成为一个成功的人。",
group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "带刺的玫瑰",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      美丽的小公主是国王的掌上明珠，国王用尽一切办法来满足小公主的要求。小公主很喜欢花，她梦想拥有一个最美丽的大花园，里面种一种世界最美丽的花。小公主把自己的愿望跟国王说了，国王于是命令他的一个大臣说：“你去给我建造一个花园，这个花园不要很多种花卉，只要栽满世界上最美的一种花就够了。”
      大臣领的命令后，就开始马不停蹄地忙开发，他边让人大兴土木建筑花园，边派使者到世界各地去先取花卉。花园很快建成了，大臣从上万种花卉里选来选去，他和很多人都认为玫瑰是这世界上最美的花朵。于是，他找来成千上万的花匠，吩咐他们在花园中遍种玫瑰，并且按照药色品种栽植，图案、信道、亭榭都设计得巧夺天工，让参观者个个大为赞叹。花园建成，花开时节，大臣想，这么美的玫瑰，这么壮观的花园，公主看了一定会很高兴的。于是在作好准备后，大臣把国王和公主领到花园里。
      谁知公主看到那些玫瑰就不高兴了：“父王，这些花下都有刺，这哪里是最美丽的花呢？我不要。”说着，小公主就大闹起来。大臣忙分辨说：“国王陛下，这些玫瑰虽然有刺，可它是世界上最美的花啊！”国王见公主不高兴，不等这个大臣说完，便狠狠地责罚了他。
      不久，国王又命令另一个大臣给公主的花园引种世界上最美丽的花卉。这个大臣领了命令后，既没有派使者去奔波着满世界找花种，也没有因为花色品种而劳心费神。等到春暖花开，满园的玫瑰花香四溢时，他便国王和公主到花园来赏花。大家都为他担心，这不是要重蹈那个被杀大臣的覆辙，让国王来砍他的脑袋吗？
      当国王和公主走进玫瑰花园时，这个大臣微笑着对公主说：“公主，你瞧瞧这些棘刺多美丽呀，每丛棘刺上都开着花朵。”
      国王和公主一看高兴万分，赞赏地说：“是啊，这些棘刺是如此美丽，每丛棘刺上都盛开着这么芬芳的花朵，这可能是世界上最美丽的棘刺，也是世界上最漂亮的花朵了。”
      国王不仅丝毫没有怪罪这位大臣，见公主开心了，还给他加官晋爵。
      很多人不理解，询问这位大臣说：“同样是长着棘刺的玫瑰，为什么那位大臣被杀，而你反而受到赏赐呢？”这位大臣听了微笑着说：“虽同是玫瑰，他让公主看到的是每朵花下都有刺，而我让公主看到的是每丛刺上都有花啊！”
      
【温馨提示】

      世界万事物都有两面性，有洒满阳光的一面，也有布满阴霾的一面，就像玫瑰的每朵花下都长了刺，而每丛刺上都绽开着美丽的花朵一样。一切都取决于你自己的生活与心灵的视角。花朵下可以看见锐利的刺，而在刺上，你则能看到斟满阳光的花朵。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "鸟儿对大树的报答",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      森林里生长着各式各样的树，这些树大部分都会在春天里开花，然后在夏天或秋天里结果。
      很多鸟儿常常到森林里来，它们有时候唱歌，有进修玩耍，有时候捉虫，它们常常会在森林里呆上很长的一段时间，然后飞到外面去，飞到外面很远的地方呆上几个月再回来。
      有时候，这些鸟儿在森林里找不到食物，它们便会向结了果实的大树请求帮助，很多树拒绝了鸟儿的请求，它们舍不得献出自己的果实，即使早已看到鸟儿饥肠辘辘。
      只有一棵大树很热情地给予鸟儿们帮助，它对鸟儿说：“来吧，尽情地吃吧，填饱你们的肚子才能有力气做事！”
      这棵大树对于鸟儿们的请求，总是无私地给予帮助。
      为了感谢大树的帮助，鸟儿总是把最美的歌献给大树，总是尽自己最大的努力为大树梳妆打扮，并在外出远游的时候，把大树的种子带到世界各地。
      
【温馨提示】

      很多年后，当大树的家庭遍布世界各地，变得兴旺发达的时候，那些曾经拒绝给鸟儿们以帮助的树才明白：原来帮助别人的时候，也是帮助自己。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "沙漠之树",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有两个人，都在一片荒漠上栽上了一片胡扬树苗。
      苗子成活后，其中一个人每隔三天，都要挑起水桶，到荒漠中来，一棵一棵地给他的那些树苗浇水。不管是烈日炎炎，还是飞沙走石，那人都会雷打不动地挑来一桶一桶的水，一一浇他的那些树苗。有时刚刚下过雨，他也会来，锦上添花地给那些树苗再浇一瓢。老人说，沙漠里的水漏得快，别看这么三天浇一次，树根其实没吮吸到多少水，水都从厚厚的沙层中漏掉了。
      而另一个人就悠闲得多了。树苗刚栽下去的时候，他来浇过几次水，等到那些树苗成活后，他就来得很少了，即使来了，也不过是到他栽的那片幼林中去看看，发现有被风吹倒的树苗的顺手扶一把，没事儿的时候，他就在那片树苗中背着手悠闲地走走，不浇一点儿水，也不培一把土，人们都说，这人栽下的那片树，肯定成不了林。
      过了两年，两片胡扬林树苗都长得有茶杯粗了，忽然有一夜，狂风从大漠深处卷着一柱柱的沙尘飞来，飞沙走石，电闪雷鸣，狂风卷着滂沱大雨肆虐一了夜。第二天风停的时候，人们到那两片细林里一看，不禁十分惊讶：原来辛勤浇水的那个人的树几乎全被暴风给刮倒了，有许多的树几乎被暴风连根拔了出来，摔折的树枝，倒地的树干，被拔出的一蓬蓬黑的根须，几乎惨不忍睹。而那个悠闲的不怎么给树浇水的人的林子，除了一些被风撕掉的树叶和一些被折断的树枝外，几乎没有一棵被风吹倒或者吹歪的。
      大家都大惑不解。
      那人微微一笑说：“他的树这么容易就被风暴给毁了，就是因为他的树浇水浇得太勤，施肥施得太勤了。”
      人们更迷惑不解了，难道辛勤为树施肥浇水是个错误吗？
      那人顿了顿，叹了口气说：“其实树跟人是一样的，对它太殷勤了，就培养了它的惰性，你经常给它浇水施肥，它的根就不往泥土深处扎，只在地表浅处盘来盘去。根扎得那么浅，怎么能经得起风雨呢？如果像我这样，把它们栽活后，就不再去理睬它，地表没有水和肥料供它们吮吸，逼它们不得不拼命向下扎根，恨不得把自己的根穿过沙土层，一直扎进地底下的泉源中去，有这么深的根，我何愁这些树不枝叶繁茂，何愁这些树会轻易就被暴风刮倒呢？”
      
【温馨提示】

      别给生命以适合的温床，生命的温床上只能诞生生命的灾难。要想使你生命之树能根深叶茂顶天立地，那就不能给它太足的水分和肥料带迫它奋力向下自己扎根。不管是一棵草，一棵树，怎么的条件就会造成怎的命运。温床上是长不出参天大树的，襁褓里藏着的绝不是伟人。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "生命中的大石头",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一天，时间管理专家为一群商学院学生讲课。他现场做了演示，给学生们留下一生难以磨灭的印象。站在那些高智商高学历的学生前面，他说：“我们来个小测验”，拿出一个一加仑的广口瓶放在他面前的桌上。随后，他取出一堆拳头大小的石块，仔细地一块块放进玻璃瓶里。直到石块高出瓶口，再也放不下了，他问道：“瓶子满了吗？”所有学生应道：“满了”。时间管理专家反问：“真的？”他伸手从桌下拿出一桶砾石，倒了一些进去，并敲击玻璃瓶壁使砾石填满下面石块的间隙。“现在瓶子满了吗？”他第二次问道。但这一次学生有些明白了，“可能还没有”，一位学生应道。“很好！”专家说。他伸手从桌下拿出一桶沙子，开始慢慢倒进玻璃瓶。沙子填满了石块和砾石的所有间隙。他又一次问学生：“瓶子满了吗？”“没满！”学生们大声说。他再一次说：“很好。”然后他拿过一壶水倒进玻璃瓶直到水面与瓶口平。抬头看着学生，问道：“这个例子说明什么？”一个心急的学生举手发言：“它告诉我们：无论你的时间表多么紧凑，如果你确实努力，你可以做更多的事！”大多数人都是这样回答。
      但是，时间管理专家却否定了该学生的答案，时间管理专家说，“那不是它真正的意思。这个例子告诉我们：如果你不是先放大石块，那你就再也不能把它放进瓶子里。那么，什么是你生命中的大石块呢，你的信仰，教育，梦想，或是和我一样，教育指导其他人？切切记得先去处理这些‘大石块’，否则，一辈子你都不能做到。”
      
【温馨提示】

      一个人要有人生目标，不管是远大的还是现实的渺小的，为目标奋斗人生才有意义。那么，今天你正在阅读这篇短文，可曾试着问自己这个问题：我今生的“大石头”是什么？然后，请把它们先放进你人生的瓶子。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "“傻子”阿甘的大智慧",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      上周课堂上学生观看了《阿甘正传》的片断，阿甘是一个弱智的，头脑简单的，想问题单纯的，目标单一，行动始终如一的傻瓜……结果，他成功了！为什么？
      老子说：“少则得，多则惑。”
      当一群孩子要欺负阿甘的时候，他的女伴告诉他，快跑！脚跛的他单纯地听从了，没命地跑，快得超过了正常的男孩；球场上，教练告诉他：“什么都别想，抢着球就跑！”他又单纯地听从了，结果他跑来了大学毕业证，跑成“球星”；他上越南打仗，他的上级告诉他：“遇见危险就跑！”他再次单纯地听从了，结果不但平安归来，还跑成了“国家英雄”；阿甘善于把所有的问题都简单化，简单单纯到了只剩下直奔成功。
      再聪明的人都无法完全认清世间万象，运转再快的头脑，也跟不上世界万物的变化。所以老子要求我们“以静制动”，“以不变应万变”，“大智若愚”……这样才能掌握世间万物，掌握我们自己。
      老子认为：“江海所以能为百谷王者，以其善下之。”——江海之所以能成为一切小河流的领袖，是因为他善于处在一切溪流的下游。老子还说：“知其雄，守其雌。”——知道别人的雄强，自己安于雌弱，这才是最强的强者。老子又说：“夫唯不争，故天下莫能与之争”——正因为你不争，所以天下才没有人能和你争，这才是争的最高境界，才是“大争”。
      阿甘不知道老子是何许人也，但阿甘就明白这个道理：他上越南战场，从来没有说要争取当一个英雄，结果他成了英雄，还受到总统的特别接见。而阿甘的上级，出身于军人世家的上尉，从一开始就争取当一个战争英雄，为家族争光，结果失去了双腿，被自己看不起的傻瓜阿甘救了一命。
      
【温馨提示】

      “不争”，并不是意味你根本不行动，而是要你不动声色，不显山露水，不作无谓斗争。无谓的争斗，只会消耗你的能量；逞强的行为，等于为自己树立了强敌；盲目出动，只会让自己失去方向，迷失自己。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "东东的体温表",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      东东有个神奇的体温表，它不仅可以测到体温还可以测到心情。
      东东最怕的数学考试来临时，体温表知道他很恐慌不想去考试，就把温度升上去很多，东东就因为可以请病假而不用去考试了；东东发烧的时候，却非常想出去春游，体温表得知了他期待的心情，就把体温的数字降低到正常，东东就高兴的去玩了，但结果回家后病情就加重了。
      时间一长，东东就发现了是体温表有问题，生气的对他说：“你干嘛要自作聪明啊？你就是一个体温表而已！你就明明白白告诉我事实真相就好了啊！！！你凭什么变来变去的！”这时候体温表委屈的说：“我死去的哥哥就是因为每次都告诉了你事实真相，结果就被你给摔碎了啊……”
      
【温馨提示】

      很多人在该做自己的事情的时候，越过了自己应该负责的职责，本来自己只要做这一小堆事情就好了，却偏偏要想到许多宏观的离自己职责很远的东西并在其中左右着自己，这样的出发点也许是好的，但具体的工作并不需要每个人都成为政治家，如果过了的话，那每个人实际上都没有尽到自己的本份，那是没有职业道德。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "没有退路就是出路",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      古希腊著名演说家戴摩西尼年轻的时候为了提高自己的演说能力，躲在一个地下室练习口才。由于耐不住寂寞，他时不时就想出去溜达溜达，心总也静不下来，练习的效果很差。无奈之下，他横下心，挥动剪刀把自己的头发减去一半，变成了一个怪模怪样的“阴阳头”。这样一来，因为头发羞于见人，他只得彻底打消了出去玩的念头，一心一意地练口才，演讲水平突飞猛进。正是凭借这种专心执着的精神，戴摩西尼最终成为了世界上著名的大演说家。
      1830年，法国作家雨果同出版商签订合同，半年内交出一部作品，为了确保能把全部精力放在写作上，雨果把身上所穿的毛衣以外的其他衣物全部锁在柜子里，把钥匙丢进了小湖。就这样，由于根本拿不到外出要穿的衣服，他彻底断了外出会友和游玩的念头，一头钻进小说里，除了吃饭和睡觉，从不离开书桌，结果作品提前两周脱稿。而这部小说仅用5个月时间就完成的作品，就是后来闻名于世的文学巨著《巴黎圣母院》。
      
【温馨提示】

      一个人要想干好一件事情，成就一番事业，就必须心无旁骛、全身贯注地追逐既定的目标。在漫漫人生路上，当我们难于驾驭自己的懒惰和欲望，不能专心致志地前行时，不妨斩断退路，逼着自己全力以赴地寻找出退路。往往只有不留下退路，才更容易找到出路，最终走向成功。如同中国成语故事破釜沉舟、背水一战，当你眼前没有路时，你恰恰是找到了路。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "狼的智慧",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一、狼的十大处世哲学
      1、卧薪尝胆
      狼不会为了所谓的尊严在自己弱小时攻击比自己强大的东西。
      2、众狼一心
      狼如果不得不面对比自己强大的东西，必群而攻之。
      3、自知之明
      狼也很想当兽王，但狼知道自己是狼不是老虎。
      4、顺水行舟
      狼知道如何用最小的代价，换取最大的回报。
      5、同进同退
      狼虽然通常独自活动，但狼却是最团结的动物，你不会发现有哪只狼在同伴受伤的时独自逃走。
      6、表里如一
      狼也很想当一个善良的动物，但狼也知道自己的胃只能消化肉，所以狼唯一能做的只有干干净净的吃掉每次猎物。
      7、知己知彼
      狼尊重每个对手，狼在每次攻击前都会去了解对手，而不会轻视它，所以狼一生的攻击很少失误。
      8、狼亦钟情
      公狼会在母狼怀孕后，一直保护母狼，直到小狼有独立能力。（所以狼很不满人把那些不钟情的人称之胃狼心狗肺！因为这不公平！）
      9、授狼以渔
      狼会在小狼有独立能力的时候坚决离开它，因为狼知道，如果当不成狼，就只能当羊了。
      10、自由可贵
      狼不会为了嗟来之食而不顾尊严的向主人摇头晃脑。因为狼知道，决不可有傲气，但不可无傲骨，所以狼有时也会独自哼哼自由歌。
      二、狼之“团队精神”
      多么壮丽的场面！广阔无垠的旷野上，一群狼踏着积雪寻找猎物。它们最常用的一种行进方法是单列行进，一匹挨着一匹。领头狼的体力消耗最大。作为开路先锋，他在松软的雪地上率先冲出一条小路，以便让后面的狼保存体力。头狼累了时，便会让到一边，让紧跟在身后的那匹狼接替它的位置。这样它就可以跟队尾，轻松一下，养精蓄锐，迎接新的挑战。
      三、个体与整体
      在夜里，没有哪一种声音比狼群异乎寻常的音乐般的嚎叫更阴森、凄楚、可怕而又动听的了。
      狼嚎的原因也许是为了打破一切等级界限提供时间、场合和机会。狼群的社会秩序非常牢固，每个成员都明白自己的作用和地位。我们观察狼群进食时，能看到类似屈膝行礼、鞠躬、哀叫和拥抱的声音和动作----一切都依每个成员在狼群中的地位而定。但是当狼在一起嚎叫时，一切等级界线都消失了，它们仿佛在宣告：“我们是一个整体，但是各个都与众不同，所以最好不要惹我们。”任何听过狼群奇妙的合唱的人都会证明，它们的这种信息表达得十分清楚。
      人类的组织和家庭更是如此，如果其中的每个个体的个性不是被扼杀而是被大加赞扬，那么它就更令人敬畏。每位成员都应通过发挥特有的才智和力量来肩负起对团队应尽的义务。通过表现个体的对特性以及尊重、鼓励其他成员表现自我，整个集体定会变得强大而令人敬畏。
      四、善于交流的狼
      狼是最善交际的食肉动物之一。它们并不仅仅以来某种单一的交流方式，而是随意使用各种方法。它们嚎叫、用鼻尖相互挨擦、用舌头舔、采取支配或从属的身体姿态，使用包括唇、眼、面部表情以及尾巴位置在内的复杂精细的身体语言或利用气味来传递信息。
      如果人类像狼一样努力培养并运用有效交流才能，我们能避免多少暴力、误解和失败？！
      有时候没有信任可能也有交流，然而没有表达清楚的交流则不可能有信任。家庭和其他组织、团体可以通过开城布公的沟通和交流来解决问题，没有沟通它们就会出现机能障碍。
      摘自《狼的智慧》
      
【温馨提示】

      一个人要想干好一件事情，成就一番事业，就必须心无旁骛、全身贯注地追逐既定的目标。在漫漫人生路上，当我们难于驾驭自己的懒惰和欲望，不能专心致志地前行时，不妨斩断退路，逼着自己全力以赴地寻找出退路。往往只有不留下退路，才更容易找到出路，最终走向成功。如同中国成语故事破釜沉舟、背水一战，当你眼前没有路时，你恰恰是找到了路。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "买橘子的故事",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      汤姆进公司两年了，对待工作一向认真、投入、尽职尽责。他每天准时上班，而且为自己从未受到过批评感到自豪。
      某天，汤姆去找经理理论，他说：“先生，我在公司工作了两年，认真敬业，但是我感觉自己受到了忽视。罗赫略6个月前来到公司，和我的职位一样，现在却已经升职了。”
      经理显得有点伤感，对他说:“一会儿我们再解释这个问题，你现在可不可以帮我一个忙？我想给全体员工准备一些工作午餐用的水果。街道拐角有一家水果店，请帮我看一下有没有桔子。”
      汤姆跑去后5分钟就回来了。
      经理问他：“汤姆，有桔子卖吗？”
      “先生，那里有桔子卖。”
      “价钱是多少？”
      “啊，我没有问这个。”
      “那卖的桔子够不够所有员工吃的？”
      “我也没有问这个，先生。”
      “那有其他水果卖吗？”
      “我不知道，但是我想......”
      “哦，你先请坐下稍等一会儿。”
      经理打电话叫来了罗赫略。罗赫略来了之后，经理给了他与汤姆相同的指示。10分钟后，他回来了。
      经理问他：“罗赫略，你给我带来了什么消息？”
      “先生，那里有桔子卖，数量足够全体员工吃。除此之外，他们还有香蕉、木瓜、甜瓜和芒果。桔子的售价是每公斤1.5比索。店主告诉我，如果买的多，他还能给我8%的折扣。我先预定了桔子，不过如果您选择其他水果的话，我就再回去下一次订单。”
      经理转向一脸惊讶地等在旁边的汤姆，问道：“汤姆，刚才你要和我商量什么问题？”
      “没什么，先生，我感到十分抱歉。”
      
【温馨提示】

      既然无人委以重任，我们就应当竭尽全力完成哪怕最简单的工作。每当我们做好每一件小事的时候，都有机会印上自己的个性标签。同样的小事情，有心人做出大学问，不动脑子的人只会来回跑腿而已。别人对待你的态度，就是你做事情结果的反应，像一面镜子一样准确无误，你如何做的，它就如何反射回来。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "突围",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      出生于美国的普拉格曼连高中也没有读完，却成为一位非常著名的小说家。在他的长篇小说授奖典礼上，有位记者问道：你事业成功关键的转折点是什么?大家估计，他可能会回答是童年时母亲的教育，或者少年时某个老师特别的栽培。然而出人意料的是，普拉格曼却回答说，是二战期间在海军服役的那段生活：1944年8月一天午夜，我受了伤。舰长下令由一位海军下士驾一艘小船趁着夜色送身负重伤的我上岸治疗。很不幸，小船在那不勒斯海迷失了方向。那位掌舵的下士惊慌失措，想拔枪自杀。我劝告他说：你别开枪。虽然我们在危机四伏的黑暗中漂荡了四个多小时，孤立无援，而且我还在淌血……不过，我们还是要有耐心……说实在的，尽管我在不停地劝告那位下士，可连我自己都没有一点信心。但还没等我把话说完，突然前方岸上射向敌机的高射炮的爆炸火光闪亮了起来，这时我们才发现，小船离码头不到三海里。普拉格曼说：那夜的经历一直留在我的心中，这个戏剧性的事件使我认识到，生活中有许多事被认为不可更改的不可逆转的不可实现的，其实大多数时候，这只是我们的错觉，正是这些“不可能”才把我们的生命“围”住了。一个人应该永远对生活抱有信心，永不失望。即使在最黑暗最危险的时候，也要相信光明就在前头……二战后，普拉格曼立志成为一个作家。开始的时候，他接到过无数次的退稿，熟悉的人也都说他没有这方面的天分。但每当普拉格曼想要放弃的时候，他就想起那戏剧性的一晚，于是他鼓起勇气，一次次突破生活中各种各样的“围”，终于有了后来眩目的灿烂和辉煌。
      另一个故事：一天早晨，电报收发员卡纳奇来到办公室的时候，得知由于一辆被撞毁的车子阻塞了道路，铁路运输陷入瘫痪。更要命的是，铁路分段长司各脱不在。按照条例，只有铁路分段长才有权发调车令，别人这样做会受到处分，甚至被革职。车辆越来越多，喇叭声、行人的咒骂声此起彼伏，有人甚至因此动起手来。“不能再等下去了。”卡纳奇想。他毅然发出了调车电报，上面签着司各脱的名字。司各脱终于回来了，此时阻塞的铁路已畅通无阻，一切顺利如常。不久，司各脱任命卡纳奇为自己的私人秘书，后来司各脱升职后，又推荐卡纳奇做了这一段铁路的分段长。发调车令属于司各脱的职权范围，其他人没人敢突破这个“围”，卡纳奇这样做了，结果他成功了。
      
【温馨提示】

      每个人其实都有着这样那样的“围”：主观上的认识上的偏见，个性上的不足，客观上的陈规陋习等都制约着我们实现生命价值的最大化。如果我们想在一生中有所作为，我们就必须要学会不停地突围。一个人要突破各种各样的“围”，不是一件容易的事。首先，我们要有识“围”的智慧。有的“围”是明摆着的，我们一看就知道它妨碍着我们走向远方。但有的“围”是“糖衣炮弹”，你看不到它对你的妨碍，或许你看到了也会有意无意地纵容它挤占心灵的地盘。其次，我们要有破“围”的实力。要突破主观的“围”，我们只需依赖意志；突破客观的“围”，则必须依靠人才、能力了。比起前者，后者的获得更艰难，付出的人生代价也更惨重。突围是我们给予自己的最好的礼物：如果把我们向往的生活比做一个小岛，突围则是一条平静的航道；如果把我们的生命化做一块土地，突围就是那粒通向秋天的种子；如果把我们的人生比做天空，突围就是那轮光芒四射的太阳……一个人可以出身贫贱，可以遭受屈辱，但绝对不能缺少突围的精神，没有这种精神，你就会失去行走的能力，永远也抵达不了本来可以抵达的人生的大境界。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "诸葛亮给子书",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      “夫君子之行，静以修身，俭以养德；非澹泊无以明志，非宁静无以致远。夫学须静也，才须学也；非学无以广才，非志无以成学。怠慢则不能励精，险躁则不能冶性。年与时驰，意与岁去，遂成枯落，多不接世。悲守穷庐，将复何及！”
      一：宁静的力量
      “静以修身”、“非宁静无以致远”、“学须静也”。
      诸葛亮忠告孩子宁静才能够修养身心，静思反省。不能够静下来，则不可以有效的计划未来，而且学习的首要条件，就是有宁的环境。现代人大多数终日忙碌，你是否应在忙乱中静下来，反思人生方向？
      二：节俭的力量
      “俭以养德”。
      诸葛亮忠告孩子要节俭，以培养自己的德行。审慎理财，量入为出，不但可以摆脱负债的困扰，更可以过着纪律的简朴生活，不会成为物质的奴隶。在鼓励消费的文明社会，你有否想过节俭的好处呢？
      三：计划的力量
      “非澹泊无以明志”、“非宁静无以致远”。
      诸葛亮忠告孩子要计划人生，不要事事讲求名利，才能够了解自己的志向，要静下来，才能够细心计划将来。面对未来，你有理想吗？你有使命感吗？你有自己的价值观吗？
      四：学习的力量
      “夫学须静也”、“才须学也”。
      诸葛亮忠告孩子宁静的环境对学习大有帮助，当然配合专注的平静心境，就更加事半功倍。诸葛亮不是天才论的信徒，他相信才能是学习的结果。你有否全心全力的学习？你是否相信努力才有成就？
      五：增值的力量
      “非学无以广才”、“非志无以成学”。
      诸葛亮忠告孩子要增值先要立志，不愿意努力学习，就不能够增加自己的才干。但学习的过程中，决心和毅力非常重要，因为缺乏了意志力，就会半途而废。你有否想过，一鼓作气人多，坚持到底人少的道理？
      六：速度的力量
      “怠慢则不能励精”。
      诸葛亮忠告孩子凡事拖延就不能够快速的掌握要点。计算机时代是速度的时代，样样事情讲求效率，想不到一千八百多年前的智能，也一样不谋而合。快人一步，不但理想达到，你有否想过，有更多时间去修正及改善？
      七：性格的力量
      “险躁则不能冶性”。
      诸葛亮忠告孩子太过急躁就不能够陶冶性情。心理学家说：“思想影响行为，行为影响习惯，习惯影响性格，性格影响命运。”诸葛亮明白生命中要做出种种平衡，要“励精”，也要“冶性”。你要提升自己性格的品质吗？
      八：时间的力量
      “年与时驰”、“意与岁去”。
      诸葛亮忠告孩子时光飞逝，意志力又会随着时间消磨，“少壮不努力，老大徒伤悲”，“时间管理”是个现代人的观念，细心想一想，时间不可以被管理，每天二十四小时，不多也不少，惟有管理自己，善用每分每秒。请你想一想，你有蹉跎岁月吗？
      九：想象的力量
      「遂成枯落」、「多不接世」、「悲守穷庐」、「将复何及」。诸葛亮忠告孩子时光飞逝，当自己变得和世界脱节，才悲叹蹉跎岁月，也于事无补。要懂得居安思危，才能够临危不乱。想象力比知识更有力量。你有没有从大处着想，小处着手，脚踏实地，规划是人生呢？
      十：精简的力量
      以上诸葛亮写给儿子的一封信，只用了短短八十六字，精简地传递了具体的讯息。我相信精简的表达源于清晰的思想，长篇大论的内容，容易令人生厌，精简沟通更有效果。你懂得言之有物地精简沟通吗？
      
【温馨提示】

      人生攻守、贵乎奋斗！真正的智能，可以超越时空，历久常新。诸葛亮写给儿子的一封信，只用了短短八十六字，但是对于为学做人有精简而且具体的忠告——在变局中安身立命，在逆境中找到力量。不要问：“自己得到些什么？”　　应该问：“自己付出过甚么？”不要问：“自己的地位如何？”　　应该问：“自己的心地如何？”不要问：“自己有甚么信仰？”　　应该问：“自己有甚么善行？”不要问：“自己是否有学问？”　　应该问：“自己是否有行动？”不要问：“自己会怎样离世？”　　应该问：“自己怎样生活过？”",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "牙与舌",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      中国古代大哲学家老子，有一天他把弟子人叫到床边，他张开口用手指一指口里面，然后问弟子们看到了什么？在场的众弟子没有一个能答得上。
      于是老子就对他们说：“满齿不存，舌头犹在。”
      
【温馨提示】

      牙齿虽硬但它寿命不长，舌头须软，但生命力更强。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "学会放下",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      老和尚携小和尚游方，途遇一条河；见一女子正想过河，却又不敢过。老和尚便主动背该女子趟过了河，然后放下女子，与小和尚继续赶路。
      小和尚不禁一路嘀咕：师父怎么了？竟敢背一女子过河？一路走，一路想，最后终于忍不住了，说：师父，你犯戒了？怎么背了女人？老和尚叹道：我早已放下，你却还放不下！
      
【温馨提示】

      君子坦荡荡，小人常戚戚；心胸宽广，思想开朗，遇事拿得起、放得下，才能永远保持一种健康的心态。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "蜘蛛与人生",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      雨后，一只蜘蛛艰难地向墙上已经支离破碎的网爬去，由于墙壁潮湿，它爬到一定的高度，就会掉下来，它一次次地向上爬，一次次地又掉下来……
      第一个人看到了，他叹了一口气，自言自语：“我的一生不正如这只蜘蛛吗？忙忙碌碌而无所得。”于是，他日渐消沉。
      第二个人看到了，他说：这只蜘蛛真愚蠢，为什么不从旁边干燥的地方绕一下爬上去？我以后可不能像它那样愚蠢。于是，他变得聪明起来。
      第三个人看到了，他立刻被蜘蛛屡败屡战的精神感动了。于是，他变得坚强起来。
      
【温馨提示】

      有成功心态者处处都能发觉成功的力量。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "把心门打开",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有兄弟二人，年龄不过四、五岁，由于卧室的窗户整天都是密闭着，他们认为屋内太阴暗，看见外面灿烂的阳光，觉得十分羡慕。兄弟俩就商量说：“我们可以一起把外面的阳光扫一点进来。”于是，兄弟两人拿着扫帚和畚箕，到阳台上去扫阳光。等到他们把畚箕搬到房间里的时候，里面的阳光就没有了。这样一而再再而三地扫了许多次，屋内还是一点阳光都没有。正在厨房忙碌的妈妈看见他们奇怪的举动，问道：“你们在做什么？”他们回答说：“房间太暗了，我们要扫点阳光进来。”妈妈笑道：“只要把窗户打开，阳光自然会进来，何必去扫呢？”
      
【温馨提示】

      把封闭的心门敞开，成功的阳光就能驱散失败的阴暗。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "接受帮助也是尊重",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在某个小村落，下了一场非常大的雨，洪水开始淹没全村，一位神父在教堂里祈祷，眼看洪水已经淹到他跪着的膝盖了。一个救生员驾着舢板来到教堂，跟神父说：“神父，赶快上来吧！不然洪水会把你淹死的！”神父说：“不！我深信上帝会来救我的，你先去救别人好了。”过了不久，洪水已经淹过神父的胸口了，神父只好勉强站在祭坛上。
      这时，又有一个警察开着快艇过来，跟神父说：“神父，快上来，不然你真的会被淹死的！”神父说：“不，我要守住我的教堂，我相信上帝一定会来救我的。你还是先去救别人好了。”又过了一会，洪水已经把整个教堂淹没了，神父只好紧紧抓住教堂顶端的十字架。
      一架直升飞机缓缓的飞过来，飞行员丢下了绳梯之后大叫：“神父，快上来，这是最后的机会了，我们可不愿意见到你被洪水淹死！”神父还是意志坚定的说：“不，我要守住我的教堂！上帝一定会来救我的。你还是先去救别人好了。上帝会与我共在的！”
      洪水滚滚而来，固执的神父终于被淹死了……
      神父上了天堂，见到上帝后很生气的质问：“主啊，我终生奉献自己，战战兢兢的侍奉您，为什么你不肯救我！”上帝说：“我怎么不肯救你？第一次，我派了舢板来救你，你不要，我以为你担心舢板危险；第二次，我又派一只快艇去，你还是不要；第二次，我以国宾的礼仪待你，再派一架直升飞机来救你，结果你还是不愿意接受。所以，我以为你急着想要回到我的身边来，可以好好陪我。”
      
【温馨提示】

      生命中太多的障碍，皆是由于过度的固执与愚昧的无知所造成。在别人伸出援手之际，别忘了，惟有我们自己也愿意伸出手来，人家才能帮得上忙。接受他人的帮助也是尊重他人的方式！",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "贝多芬为艺术而生",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      路德维希?范?贝多芬一生中，贫困、疾病、失意、孤独等种种磨难折磨着他，其中最大的灾难是耳聋给他带来的痛苦。
      贝多芬28岁时，由于疾病，听觉就开始减退，到了48岁，再优美的歌声他也听不见了。他只能用书写的方式来和别人交流。
      即使这样贝多芬仍进行着创作。他的不朽名作――九十部交响曲的后七部，都是在失聪的情况下完成的。而其中的第三、第五、第六和第九部交响曲被认为是永恒的杰作。
      他用敏锐的观察力来感受人类、社会和大自然。为了起草一部曲子，他经常花几个月甚至几年的时间反复推敲，精心锤炼。例如第五交响曲的创作，他就花了八年的时间。
      贝多芬在给他的兄弟卡尔和约翰的信中倾诉了耳聋给他带来的莫大的痛苦以及他战胜疾病的决心：“在我身旁的人都能听到远处的笛声，而我却听不到，这是何等的耻辱啊！这样的情景曾把我推到了绝望的边缘，几乎迫使我结束了自己的生命。但是，我的艺术，只有我的艺术要我活下去。”贝多芬在这种困境中曾大声疾呼：“我要扼住命运的咽喉，它不能使我完全屈服！”为了艺术，他牺牲了平庸的私欲，战胜了一切不幸。他说：“牺牲，永远把一切人生的愚昧为你的艺术去牺牲！艺术，这是高于一切的上帝。”
      
【温馨提示】

      对强者来说，苦难也是一种财富；对弱者来说，苦则是人生的障碍。“艰难困苦，玉汝于成”。苦难可以提高人格；可以磨练一个人的意志；可以激发人的勇气。成功征服苦难后，人就能真正感受到生活的甘甜、成功的喜悦。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "盖茨背圣经",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在美国西雅图的一所著名教堂里，有一位德高望重的牧师――戴尔?泰勒。有一天，他向教会学校一个班的学生们先讲了下面这个故事。
      那年冬天，猎人带着猎狗去打猎。猎人一枪击中了一只兔子的后腿，受伤的兔子拼命地逃生，猎狗在其后穷追不舍。可是猎狗知道追不上了，只好悻悻地回到猎人身边。猎人气急败坏地说：“你真没用，连一只受伤的兔子都追不到！”猎狗听了很不服气地辩解道：“我已经尽力而为了呀！”再说兔子带着枪伤成功地逃生回家了，兄弟们都围过来惊讶地问它：“那只猎狗很凶呀，你又带了伤，是怎么甩掉它的呢？”兔子说：“它是尽力而为，我是竭尽全力呀！它没追上我，最多挨一顿骂，而我若不竭尽全力地跑，可就没命了呀！”
      泰勒牧师讲完故事之后，又向全班郑重其事地承诺：谁要是能背出《圣经?马太福音》中第五章到第七章的全部内容，他就邀请谁去西雅图的“太空针”高塔餐厅参加免费聚餐会。
      《圣经?马太福音》中第五章到第七章的全部内容有几万字，而且不押韵，要背诵其全文无疑有相当大的难度。尽管参加免费聚餐会是许多学生梦寐以求的事情，但是几乎所有的人都浅尝则止，望而却步了。
      几天后，班中一个11岁的男孩，胸有成竹地站在泰勒牧师的面前，从头到尾地按要求背诵下来，竟然一字不漏，没出一点差错，而且到了最后，简直成了声情并茂的朗诵。泰勒牧师比别人更清楚，就是在成年的信徒中，能背诵这些篇幅的人也是罕见的，何况是一个孩子。泰勒牧师在赞叹男孩那惊人记忆力的同时，不禁好奇地问：“你为什么能背下这么长的文字呢？”这个男孩不假思索地回答道：“我竭尽全力。”
      16年后，这个男孩成了世界著名软件公司的老板——他就是比尔?盖茨。
      
【温馨提示】

      每个人都有极大的潜能。正如心理学家所指出的，一般人的潜能只开发了2－8%左右，像爱因斯坦那样伟大的大科学家，也只开发了12%左右。一个人如果开发了50%的潜能，就可以背诵400本教科书，可以学完十几所大学的课程，还可以掌握二十来种不同国家的语言。这就是说，我们还有90%的潜能还处于沉睡状态。谁要想出类拔萃、创造奇迹，仅仅做到尽力而为还远远不够，必须竭尽全力才行。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "永远的坐票",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个人经常出差,经常买不到对号入座的车票.可是无论长途短途,无论车上多挤,他总能找到座位.
      他的办法其实很简单,就是耐心地一节车厢一节车厢找过去.这个办法听上去似乎并不高明,但却很管用.每次,他都做好了从第一节车厢走到最后一节车厢的准备,可是每次他都用不着走到最后就会发现空位.他说,这是因为像他这样锲而不舍找座位的乘客实在不多.经常是在他落座的车厢里尚余若干座位,而在其他车厢的过道和车厢接头处,居然人满为患.
      他说,大多数乘客轻易就被一两节车厢拥挤的表面现象迷惑了,不大细想在数十次停靠之中,从火车十几个车门上上下下的流动中蕴藏着不少提供座位的机遇;即使想到了,他们也没有那一份寻找的耐心.眼前一方小小立足之地很容易让大多数人满足,为了一两个座位背负着行囊挤来挤去有些人也觉得不值.他们还担心万一找不到座位,回头连个好好站着的地方也没有了.
      与生活中一些安于现状不思进取害怕失败的人,永远只能滞留在没有成功的起点上一样,这些不愿主动找座位的乘客大多只能在上车时最初的落脚之处一直站到下车.
      
【温馨提示】

      生活真是有趣，如果你只接受最好的，你经常会得到最好的。自信、执着、富有远见、勤于实践会让你握有一张人生之旅永远的坐票。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "格莱斯顿的耐性",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      炎热的夏天，在英国一大型教堂里，牧师正在那里布道，但由于长长的布道和闷热的原因，许多教徒开始变得昏昏欲睡。可是，有一位绅士，他看上去却精神抖擞。他腰背挺直，正专注地坐在那里听着牧师讲道。
      出了教堂，有人向这位绅士问道：“先生，每个人都在打瞌睡，为什么你还能听得那么认真呢？”
      绅士微笑着说：“老实说，听这样的讲道，我也很想打瞌睡。可我忽然想到，我何不把它用来试试自己的耐性呢？事实证明，我的耐性非常好。我想，以这种耐心去面对工作中的各种困难，还有什么不能解决呢？”
      知道这位绅士是谁吗？他就是后来鼎鼎有名的英国首相格莱斯顿。
      
【温馨提示】

      世上没有绝对不好的事情，只有绝对不好心态的人。连自己心态都调整不好的人，他们又怎么能处理好比这心态更为复杂的事情呢！",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "巴黎的画廊咖啡厅",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个默默无闻的年轻画家，觉得自己的作品十分优秀，但是一直没有找到合适的机会向大众展示。毕竟在巴黎街头，像他这样的画家实在太多了。于是，一气之下，他倾尽了所有家产，又向朋友借了一些钱，在艺术街里开办了一间画廊，专门展示自己的作品。他以为这么一来，自己的作品很快就能有很高的知名度，为自己赢得显赫的名声和大量的财富。但是哪里知道，等他在艺术街一个不显眼的角落（租金已经高到了他能够承受的极限）开办了这间画廊之后，他才发现了一个残酷的事实，原来在艺术街上已经有了太多的画廊，除了十几家装饰特别华丽的知名画廊外，像他这种小画廊根本没有什么人进门，更不用说吸引投资人了。就这样，在苦苦守望了几个月后，他只能无奈地决定关闭这间曾经寄予了无限希望，但仍然门庭冷落的画廊。一天，他坐在街道上惟一的咖啡馆里，望着周围热闹的客人，自己苦闷地喝着咖啡。突然，一个灵感涌进了他的脑海之中。也许，开一间这样的咖啡馆，生意应该不错。但自己的作品呢？他凝视着这家咖啡馆墙壁上几幅陈旧庸俗的画，思路就更加清晰了。对！就这样做！他一拍大腿，起身回家。到了家就重新忙碌起来。
      一个星期后，在这条长长的艺术街上，并没有什么大事发生，但人们行走的方式发生了变化，因为在街道一个的角落里，又出现了一间小小的咖啡馆。虽然位置并不显眼，但咖啡的香气足以吸引每一个路人，让他们在畅游艺术殿堂之后，到这里休息片刻。而且，进到里面，竟然发现墙壁上挂满了一些颇有新意的作品。就这样，他的咖啡馆前门庭若市，其中更有不少投资人。终于，一天中午，有人问起这些作品的来历。当对方知道它们出自面前这个在咖啡馆里跑堂的老板时，敬意油然而生。几个月过后，他的作品终于推广出去了，而那间小小的咖啡屋，或者说那间不同凡响的画廊已经在街上名声显赫了。
      
【温馨提示】

      在生活中，我们常常可以遇到类似的一幕。许多人将成功的目标定得非常高，但又不愿意从小事做起，一心只盼望能够一步登天，最终却是难以有所成就。事实上，对于世间的事，想获得的越多往往得到的越少；相反，有时一心耕作，不问收获，却能实现理想的目标。只要能真正明白这个道理，成功就会变得容易一些。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "善待别人就是善待自己",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个人在拥挤的车流中开着车缓缓前进，在等红灯的时候，一个衣衫褴褛的小男孩敲着车窗问他要不要买花。他刚刚递出去五块钱绿灯就亮了，后面的人便猛按喇叭催促，因此他粗暴地对问他要买什么颜色花的男孩说：“什么颜色都可以，你只要快一点就行了！”那男孩十分礼貌地说：“谢谢您，先生！”
      在开了一小段路后，他有些良心不安，他粗暴无礼的态度，却得到对方如此有礼的回应。于是他把车停在路边，回头走向孩子表示歉意，并且给了他五块钱，要男孩自己买一束花送给喜欢的人，这个孩子笑了笑并道谢接受了。
      当他回去发动车子时，发现车子出了故障，动不了了，在一阵忙乱之后，他决定步行去找拖车帮忙。正在思索时，一辆拖车竟然已经迎面驶来，他大为惊讶。司机笑着对他说：“有一个小孩给了我十块钱，要我开过来帮你，还写了一张纸条。”他打开一看，上面写着：“这代表一束花。”
      
【温馨提示】

      勇于认错的人才是真正的智者，你的善意可能立即得到回报，而内心的释怀却是最好的回报。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "老婆婆与玉米",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个老婆婆在屋子后面种了一大片玉米。一个颗粒饱满的玉米说道：'收获那天，老婆婆肯定先摘我，因为我是今年长得最好的玉米！'可是收获那天，老婆婆并没有把它摘走。'明天，明天她一定会把我摘走！'很棒的玉米自我安慰着……第二天，老婆婆又收走了其它一些玉米，可唯独没有摘这个玉米。'明天，老婆婆一定会把我摘走！'棒玉米仍然自我安慰着……可是……从此以后，老婆婆再也没有来过。直到有一天，玉米绝望了，原来饱满的颗粒变得干瘪坚硬。可是就在这时，老婆婆来了，一边摘下它，一边说：'这可是今年最好的玉米，用它作种子，明年肯定能种出更棒的玉米！'
      
【温馨提示】

      也许你一直都很相信自己，但你是否有耐心在绝望的时候再等一下！",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "火把的启示",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个商人在翻越一座山时，遭遇了一个拦路抢劫的山匪。商人立即逃跑，但山匪穷追不舍，走投无路时，商人钻进了一个山洞里，山匪也追进山洞里。在洞的深处，商人未能逃过山匪的追逐，黑暗中，他被山匪逮住了，遭到一顿毒打，身上的所有钱财，包括一把准备为夜间照明用的火把，都被山匪掳去了，幸好山匪并没有要他的命。之后，两个人各自寻找着洞的出口，这山洞极深极黑，且洞中有洞，纵横交错。
      山匪将抢来的火把点燃，他能看清脚下的石块，能看清周围的石壁，因而他不会碰壁，不会被石块绊倒，但是，他走来走去，就是走不出这个洞，最终，他力竭而死。商人失去了火把，没有了照明，他在黑暗中摸索行走得十分艰辛，他不时碰壁，不时被石块绊倒，跌得鼻青脸肿，但是，正因为他置身于一片黑暗之中，所以他的眼睛能够敏锐地感受到洞里透进来的微光，他迎着这缕微光摸索爬行，最终逃离了山洞。
      
【温馨提示】

      世间事大多如此，许多身处黑暗的人，磕磕绊绊，最终走向了成功；而另一些人往往被眼前的光明迷失了前进的方向。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "适时调整",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一天，英国维多利亚女王和丈夫吵架了，丈夫独自先回到卧室，把门锁上不出来。
      女王回卧室时，只好敲门。
      丈夫问：“谁？”
      维多利亚傲然地回答：“女王。”
      里面既不开门也无声息。她只好再次敲门。
      丈夫又问：“谁？”
      女王回答：“维多利亚。”
      里面还是没有动静。女王再次敲门。
      丈夫再问：“谁？”
      女王学乖了，柔声地回答：“你的妻子。”
      这一次，门打开了。
      
【温馨提示】

      聪明的人总是根据情境的变化，做出适时的调整。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "爱跑圈的爱地巴",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在古老的西藏，有一个叫爱地巴的人，每次生气和人起争执的时候，就以很快的速度跑回家去，绕着自己的房子和土地跑三圈，然后坐在田边喘气。爱地巴工作非常勤劳努力，他的房子越来越大，土地也越来越广。但不管房地有多广大，只要与人争论而生气的时候，他就会绕着房子和土地跑三圈。“爱地巴为什么每次生气都绕着房子和土地跑三圈呢？”所有认识他的人，心里都感到疑惑，但是不管怎么问他，爱地巴都不愿意明说。直到有一天，爱地巴很老耸的房地也已经太大了，他生了气，拄着拐杖艰难地绕着土地和房子转，等他好不容易走完三圈，太阳已经下山了，爱地巴独自坐在田边喘气。他的孙子在身边恳求他：“阿公！您已经这么大年纪了，这附近地区也没有其他人的土地比您的更广大，您不能再像从前，一生气就绕着土地跑了。还有，您可不可以告诉我您一生气就要绕着土地跑三圈的秘密？”爱地巴终于说出隐藏在心里多年的秘密，他说：“年轻的时候，我一和人吵架、争论、生气，就绕着房地跑三圈，边跑边想自己的房子这么小，土地这么少，哪有时间去和人生气呢？一想到这里，气就消了，把所有的时间都用来努力工作。”孙子问道：“阿公！您年老了，又变成最富有的人，为什么还要绕着房子和土地跑呢？”爱地巴笑着说：“我现在还是会生气，生气时绕着房子和土地跑三圈，边跑边想自己的房子这么大，土地这么多，又何必和人计较呢？一想到这里，气就消了。”
      
【温馨提示】

      忍耐正是涵养性情、磨练志气、坚定决心的不二法门。发怒是最容易的事，而忍气吞声也并不难。忍者才能冷静地面对现实，经常生气的人才是逃避现实的懦夫。世上唯有莽撞使人失败误事，忍耐才是无法攻破的城堡。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "还有一个苹果",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位老和尚，他身边有一帮虔诚的弟子。这一天，他嘱咐弟子每人去南山打一担柴回来。弟子们匆匆行至离山不远的河边，人人目瞪口呆。只见洪水从山上奔泻而下，无论如何也休想渡河打柴了。无功而返，弟子们都有些垂头丧气。唯独一个小和尚与师傅坦然相对。师傅问其故，小和尚从怀中掏出一个苹果，递给师傅说，过不了河，打不了柴，见河边有棵苹果树，我就顺手把树上唯一的一个苹果摘来了。后来，这位小和尚成了师傅的衣钵传人。
      
【温馨提示】

      世上有走不完的路，也有过不了的河。过不了的河掉头而回，也是一种智慧。但真正的智慧还要在河边做一件事情：放飞思想的风筝，摘下一个“苹果”。历览古今，抱定这样一种生活信念的人，最终都实现了人生的突围和超越。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "做一棵苹果树",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一棵苹果树，终于结果了。第一年，它结了10个苹果，9个被拿走，自己得到1个。对此，苹果树愤愤不平，于是自断经脉，拒绝成长。第二年，它结了5个苹果，4个被拿走，自己得到1个。“哈哈，去年我得到了10％，今年得到20％！翻了一番。”这棵苹果树心理平衡了。但是，它还可以这样：继续成长。譬如，第二年，它结了100个果子，被拿走90个，自己得到10个。很可能，它被拿走99个，自己得到1个。但没关系，它还可以继续成长，第三年结1000个果子……
      其实，得到多少果子不是最重要的。最重要的是，苹果树在成长！等苹果树长成参天大树的时候，那些曾阻碍它成长的力量都会微弱到可以忽略。
      
【温馨提示】

      不要太在乎果子，成长是最重要的。",
            group1));

            group1.Items.Add(new SampleDataItem("Group-4-Item-3",
            "摘苹果",
            "Item Subtitle: 3",
            "Assets/title/1.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一家公司想招聘营销部经理，招聘消息一出，很多人都来面试，最后有三个人进入了由公司老总亲自主持的最后一轮考试。考试那天，老总开车把三位年轻人拉到一座果园里，他没有给他们出难题，而是指着三棵高大的苹果树说：“你们每人一棵树，看谁摘的苹果最多，谁就能成为本公司营销部经理。”
      老总刚说完，三个人马上行动。第一个人身高臂长，站在树下，上下左右开弓，不一会儿就摘了很多苹果。第二个人身材灵巧，就像猴子一样爬上树，闪展腾挪，眨眼间，也摘了不少苹果。第三个人生得又矮又胖，尽管他很着急，但摘的苹果显然落后于其它二人。
      第三个人大脑急速地转动：“要是有架梯子就好了，可哪有梯子呢？果园门卫大爷那儿会不会有？”想到这儿，他立即跑到门卫室，诚恳地向门卫大爷说明情况。刚才老总领他们进来时，只有第三个热情地和大爷打过招呼，大爷显然对他的印象很好。他领着第三个人来到门卫室后面，果然有架铝合金梯子摆在那里。他谢过大爷后，搬着梯子跑回果园。有了梯子，他变得游刃有余，摘起果子来随心所欲。
      这时，第二、二个人都遇到了难题。第一个人虽然生得高大，却怎么也够不到高处的累累硕果。第二个人虽然身手敏捷，却不敢爬到细枝上去摘。这时他俩也想到了用梯子，可跑出去找了一圈，谁也没找到梯子的影子。
      等他们气喘吁吁地跑回来，老总对他们说：“不用再比了，我宣布：第三个人被聘为营销部经理。虽然高大第三人比不过第一个人，灵巧比不过第二个人，但他面对困难时，能迅速地长到解决办法，打开局面，这是一个营销部经理最可贵的素质。”
      
【温馨提示】

      一个人的力量总是有限的，所以，当我们使出浑身解数仍无法解决问题时，别忘了好好利用以自身以外的资源，它往往能带给我们意想不到的结果。",
            group1));
            this.AllGroups.Add(group1);
            #endregion

            #region 第2章
            var group2 = new SampleDataGroup("Group-2",
            "第2章",
            "Group Subtitle: 2",
            "Assets/title/2.png",
            "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
"毛毛虫的故事",
"Item Subtitle: 3",
"Assets/title/2.png",
"Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
@"
      第一只毛毛虫
      话说第一只毛毛虫，有一天爬呀爬呀过山河，终于来到这棵苹果树下。他并不知道这是一棵苹果树，也不知树上长满了红红的苹果。当他看到同伴们往上爬时，不知所以的就跟着往上爬。没有目的，不知终点，更不知生为何求、死为何所。他的最后结局呢？也许找到了一颗大苹果，幸福的过了一生；也可能在树叶中迷了路，颠沛流离糊涂一生。不过可以确定的是，大部分的虫都是这样活着的，也不去烦恼什么是生命意义，倒也轻松许多。
      第二只毛毛虫
      有一天，第二只毛毛虫也爬到了苹果树下。他知道这是一棵苹果树，也确定他的「虫生目标」就是找到一棵大苹果。问题是....他并不知道大苹果会长在什么地方？但他猜想：大苹果应该长在大枝叶上吧！于是他就慢慢地往上爬，遇到分支的时候，就选择较粗的树枝继续爬。当然在这个毛虫社会中，也存在考试制度，如果有许多虫同时选择同一个分支，可是要举行考试来决定谁才有资格通过大树枝。幸运的，这只毛毛虫一路过关斩将，每次都能第一志愿的选上最好的树枝，最后他从一枝名为「大学」的树枝上，找到了一颗大苹果。不过他发现这颗大苹果并不是全树上最大的，顶多只能称是局部最大。因为在它的上面还有一颗更大的苹果，号称「老板」，是由另一只毛毛虫爬过一个名为「创业」的树枝才找到的。令他泄气的是，这个创业分支是他当年不屑于爬的一棵细小的树枝。
      第三只毛毛虫
      接着，第三只毛毛虫也来到了树下。这只毛毛虫相当难得，小小年纪，却自己研制了一副望远镜。在还未开始爬时，就先利用望远镜搜寻一番，找到了一棵超大苹果。同时，他发觉当从下往上找路时，会遇到很多分支，有各种不同的爬法；但若从上往下找路时，却只有一种爬法。他很细心的从苹果的位置，由上往下反推至目前所处的位置，记下这条确定的路径。于是，他开始往上爬了，当遇到分支时，他一点也不慌张，因为他知道该往那条路走不必跟着一大堆虫去挤破头。譬如说，如果他的目标是一颗名叫「教授」的苹果，那应该爬「升学」这条路；如果目标是「老板」，那应该爬「创业」这分支；若目标是「政客」，也许早就该爬「厚黑之道」这条路了。最后，这只毛毛虫「应该」会有一个很好的结局，因为他已具备了「先觉」的条件了。但也许会有一些意外的结局出现，因为毛毛虫的爬行相当缓慢，从预定苹果到抵达时，需要一段时间。当他抵达时，也许苹果已被别的虫捷足先登，也许苹果已熟透而烂掉了....
      第四只毛毛虫
      第四只毛毛虫可不是一只普通的虫，同时具有先知先觉的能力。他不仅先觉知道自己要何种苹果，更先知──知道未来苹果将如何成长。因此当他带着那「先觉」的望远镜时，他的目标并不是一颗大苹果，而是一芽含苞待放的苹果花。他计算着自己的时程，并估计当他抵达时，这朵花正好长成一颗成熟的大苹果，而且他将是第一个钻入大快朵颐的虫。果不其然，他获得所应得的，从此过着幸福快乐的日子。
      第五只毛毛虫
      毛毛虫的故事本来应该到此结束了。因为所有故事的结局都必须是正面的且富有教育意义。但仍有不少读者好奇：第五只毛毛虫到底怎么了？其实他什么也没做，就在树下躺着纳凉，而一颗颗大苹果就从天而降在他的身边。因为树上某一大片树枝早就被他的家族占领了。他的爷爷、爸爸、哥哥们盘据在某一树干上，禁止他虫进入。然后苹果成熟时，就一颗颗的丢给底下的子孙们捡食。奉劝诸位，如果你不是含着金汤匙出生的，可不要妄想检到大苹果，因为反而会被砸死的。
      
【温馨提示】

      ",
group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "煮“石头汤”",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个饿汉来到富人家门口对他说：“我带了些石头，想用一下你的锅煮点石头汤喝。”富人很奇怪。石头怎么能煮汤喝？于是，富人让他进屋，借他一口锅。饿汉把石头放进锅里。煮汤得加水吧，富人给了他一些水。煮汤得加盐吧，富人又给他一些盐。煮汤还需要调料吧，富人又给他一些调料。就这样，饿汉喝上了有滋有味的汤。
      饿汉知道单凭自己的乞求，富人是不可能给他美味可口的汤喝的，于是他就想出了煮“石头汤”这个绝妙主意，先是得到了锅，然后是水，盐，调料。最后终于喝上了美味可口的“石头汤”。
      
【温馨提示】

      世上的事情，办法总是多于困难。只要我们认准一个合理的目标并为之努力，在困难面前就会释放出超常的智慧和潜能。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "你的人生由你决定",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一位智者，他以有先知能力而著称。有一天，两个年轻男子去找他。这两个人想愚弄这位智者，于是想出了下面这个点子：他们中的一个在右手里藏一只雏鸟，然后问这位智者：“智慧的人啊，我的右手有一只小鸟，请你告诉我这只鸟是死的还是活的？”你想想，如果这位智者说：“鸟是活的”，那么拿着小鸟的人不经意地将手一握，把小鸟弄死，用这种方式来愚弄智者。如果他说：“鸟是死的”，那么这个人只需把手松开，小鸟就会振翅一飞。两个人认为他们万无一失，因为他们觉得问题只有这两种答案。
      在他们确信自己的计划滴水不漏之后，就起程去了智者家，想跟他玩玩这个把戏。他们很快见到了智者，并提出了准备好的问题：“智慧的人啊，你认为我手里的小鸟是死的还是活的？”其中一人问道。老人久久地看着他们，微笑着回答：“告诉你，我的朋友，这只鸟是死是活完全取决于你的手！”
      
【温馨提示】

      你的人生由你自己决定，你的人生的好坏也完全是由你自己决定，你就是作决定的人。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "成功从来没有时间表",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      这是一名德国女人，２０岁那年，因为天生丽质加上杰出的演技，她被当时的纳粹头目相中，“钦点”成战争专用宣传工具。几年之后，德国战败，她因此受到牵连，被判入狱四年。刑满释放之后，一晃十几年过去，她的身份，仍然走不出囚犯的影子，没人敢启用她，没人敢收容她，甚至，没人敢娶她，年近半百，她依然独来独往、形单影只。
      她的五十岁生日就这样悄然而凄然地来到了。那一天，她大醉了一场，醒来之后，突然做出了一个谁也意想不到的决定：只身深入非洲原始部落，采写拍摄独家新闻。这之后的两年，她用照片，一举奠定了她在国内摄影界的地位。
      她的奋斗精神和曲折经历深深吸引了一位３０岁的小伙子，他和她是同行。他们远离人间的一切是是非非，相敬如宾地恩爱，出入战火和内乱交困的非洲部落，深入大西洋海底世界探险，书写了一段浪漫而美丽的爱情。
      为了使自己的拍摄才华与神秘的海底世界融为一体，在６８岁那年，她开始学潜水。随后，她的作品集中增添了瑰丽多彩的海洋记录，这段海底拍摄生涯一直延续到她百岁高龄。最后，她以一部长达４５分的精湛短片《水下世界》写下了纪录电影的一个里程碑，也为自己的艺术生命划上了一个圆满的句号。
      这位前半生失足、后半生瑰丽、充满传奇色彩的女性，就是被英国《时代周刊》评为２０世纪最有影响的１００位艺术家中惟一的女性。她的名字叫莱妮.丽劳斯塔尔。
      
【温馨提示】

      成功没有时间表。只要时刻保持一腔自信、一颗不息的奋斗雄心，生命的硕果就会永远如影相随。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "不同鹦鹉的价钱",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个人去买鹦鹉，看到一只鹦鹉前标：此鹦鹉会两门语言，售价二百元。另一只鹦鹉前则标道：此鹦鹉会四门语言，售价四百元。该买哪只呢？两只都毛色光鲜，非常灵活可爱。这人转啊转，拿不定主意。结果突然发现一只老掉了牙的鹦鹉，毛色暗淡散乱，标价八百元。这人赶紧将老板叫来：“这只鹦鹉是不是会说八门语言？”店主说：“不会。”这人奇怪了：“那为什么又老又丑，又没有能力，会值这个数呢？”店主回答：“因为另外两只鹦鹉叫这只鹦鹉‘老板’。”
      
【温馨提示】

      真正的领导人，不一定自己能力有多强，只要懂信任，懂放权，懂珍惜，就能团结比自己更强的力量，从而提升自己的身价。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "音乐大师的授徒方法",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位音乐系的学生走进练习室，钢琴上，摆着一份全新的乐谱。“超高难度.....”他翻动着，喃喃自语，感觉自己对弹奏钢琴的信心似乎跌到了谷底，消磨殆尽。已经三个月了！自从跟了这位新的指导教授之后，他不知道，为什么教授要以这种方式整人。勉强打起精神。他开始用十只手指头奋战、奋战、奋战........琴声盖住了练习室外教授走来的脚步声。指导教授是个极有名的钢琴大师。授课第一天，他给自己的新学生一份乐谱。“试试看吧！”他说。乐谱难度颇高，学生弹得生涩僵滞，错误百出。“还不熟，回去好好练习！”教授在下课时，如此叮嘱学生。
      学生练了一个星期，第二周上课时正准备让教授验收，没想到教授又给了他一份难度更高的乐谱。“试试看吧”！上星期的课，教授提也没提。学生再次挣扎于更高难度的技巧挑战。第三周，更难的乐谱又出现了。同样的情形持续着，学生每次在课堂上都被一份新的乐谱所困扰，然后把它带回练习，接着再回到课堂上，重新面临两倍难度的乐谱，却怎么样都追不上进度，一点也没有因为上周的练习而有驾轻就熟的感觉。学生感到越来越不安、沮丧和气馁。教授走进练习室。学生再也忍不住了。他必须向钢琴大师提出这三个月来何以不断折磨自己的质疑。
      教授没开口，他抽出了最早的第一份乐谱，交给学生。“弹奏吧！”他以坚定的眼神望着学生。不可思议的结果发生了，连学生自己都惊讶万分，他居然可以将这首曲子弹奏得如此美妙，如此精湛！教授又让学生试了第二堂课的乐谱，学生依然呈现超高水准的表现........演奏结束，学生怔怔地看着老师，说不出话来。
      “如果，我任由你表现最擅长的部分，可能你还在练习最早的那份乐谱，就不会有现在这样的程度............”钢琴大师缓缓地说。
      
【温馨提示】

      人，往往习惯于表现自己所熟悉、所擅长的领域。但如果我们愿意回首，细细检视，将会恍然大悟：面对紧锣密鼓的工作挑战，学习苦难,难度渐升的工作压力，持续提升自我，不也就在不知不觉间养成了今日的诸般能力吗？因为，人，确实有无限的潜力。有了这层体悟与认识，会让我们更欣然乐意地面对未来更多的难题，不断磨练自己。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "你的斧头磨利了吗？",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      美国在几十年前经济非常不景气，所以年轻人的失业率很高。美国政府为了提供更多的工作机会，也为了锻炼年轻人的体魄，在落矶山里腾出一块林地，由年轻人负责伐木。工人并不利用电锯或其他有效率的工具来砍伐树木，而是使用传统的斧头，以达到强健身体的目的。
      有一位年轻人经由申请后，来到山里工作。
      第一天领到斧头，相当兴奋，到所属的林区砍树。眼看树一棵棵地倒下，他就工作得更卖力。收工时，一位老工头来验收他的成绩后，告诉年轻人：“你的表现很好，一天共砍了十五棵树，我们开办以来一天最高纪录是十六棵，你再努力一点就可以破纪录了。”
      年轻人受到激励，第二天起床得更早，匆忙盥洗吃早餐后就赶到林区砍树。努力工作一天后，老工头又来验收，发现他砍了十四棵树。年轻人自己也发现退步了离自己想破纪录的目标也越来越远。于是他下定决心，第三天要更努力，绝不能松懈下来。第三天在天色还是一片漆黑时，他就已经起床，没吃早饭就跑到林区门口，大门一开就冲进去工作。中午休息铃响，仍不愿意放下手边的工作。工头要求一定要停工了，他才勉强放下斧头。
      结果在清点后，年轻人发现他只砍了十三棵树，他有些挫折，自问为什么每天都在退步。老工头听到后笑笑说：“你是不是没有第一天那么有热忱，午餐吃太久，又休息比较久，是不是?”年轻人听了很生气地说:“我今天早餐、午餐都没吃，你居然讲我偷懒！”顺手就把斧头丢在地上，老工头马上把斧头捡起来，拉住年轻人说：“你看你这把斧刃，都已经开花了，你上次磨斧头是什么时候？”年轻人更生气地说:“我每天都这么忙，那有时间磨斧头！”
      
【温馨提示】

      我们常会利用百分之二十的时间去完成百分之八十的事，因为在充分休息及充电后，自己的效率会提高许多。所以设法均衡地安排自己的时间，就像每天在检查和磨利斧头一样，能帮助我们用更少的时间，完成更多的事。这是克服过度的忧虑与压力的好方法，也可以提高我们的生活品质。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "独一无二的玫瑰",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      小王子有一个小小的星球，星球上忽然绽放了一朵娇艳的玫瑰花。以前，这个星球上只有一些无名的小花，小王子从来没有见过这么美丽的花，他爱上这朵玫瑰，细心地呵护她。那一段日子，他以为，这是一朵人世间唯一的花，只有他的星球上才有，其他的地方都不存在。然而，等他来到地球上，发现仅仅一个花园里就有5000朵完全一样的这种花朵。这时，他才知道，他有的只是一朵普通的花。一开始，这个发现，让小王子非常伤心。但最后，小王子明白，尽管世界上有无数朵玫瑰花，但他的星球上那朵，仍然是独一无二的，因为那朵玫瑰花，他浇灌过，给她罩过花罩，用屏风保护过，除过她身上的毛虫，还倾听过她的怨艾和自诩，聆听过她的沉默…一句话，她是他独一无二的玫瑰。“正因为你为你的玫瑰花费了时间，这才使你的玫瑰变得如此重要。”一只被小王子驯服的狐狸对他说。
      这是法国名著《小王子》中一个有名的寓言故事。
      
【温馨提示】

      只有倾注了爱，亲密关系才有意义。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "小猫逃影子",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      “影子真讨厌！”小猫汤姆和托比都这样想，“我们一定要摆脱它。”然而，无论走到哪里，汤姆和托比发现，只要一出现阳光，它们就会看到令它们抓狂的自己的影子。不过，汤姆和托比最后终于都找到了各自的解决办法。汤姆的方法是，永远闭着眼睛。托比的办法则是，永远待在其他东西的阴影里。
      
【温馨提示】

      阴影和光明一样，都是人生的财富。一个小的心理问题是如何变成更大的心理问题的。可以说，一切心理问题都源自对事实的扭曲。什么事实呢？主要就是那些令我们痛苦的负性事件。因为痛苦的体验，我们不愿意去面对这个负性事件。但是，一旦发生过，这样的负性事件就注定要伴随我们一生，我们能做的，最多不过是将它们压抑到潜意识中去，这就是所谓的忘记。但是，它们在潜意识中仍然会一如既往地发挥作用。并且，哪怕我们对事实遗忘得再厉害，这些事实所伴随的痛苦仍然会袭击我们，让我们莫名其妙地伤心难过，而且无法抑制。这种疼痛让我们进一步努力去逃避。发展到最后，通常的解决办法就是这两个：要么，我们像小猫汤姆一样，彻底扭曲自己的体验，对生命中所有重要的负性事实都视而不见；要么，我们像小猫托比一样，干脆投靠痛苦，把自己的所有事情都搞得非常糟糕，既然一切都那么糟糕，那个让自己最伤心的原初事件就不是那么疼了。痛苦的事实是一个魔鬼，为了躲避这个魔鬼，干脆把自己卖给更大的魔鬼。真正抵达健康的方法只有一个——直面痛苦。直面痛苦的人会从痛苦中得到许多意想不到的收获，它们最终会变成当事人的生命财富。规划利用好现有的能力远比挖掘所谓的潜能更重要。”一个最重要的心理规律是，无论多么痛苦的事情，你都是逃不掉的。你只能去勇敢地面对它，化解它，超越它，最后和它达成和解。如果你自己暂时缺乏力量，你可以寻找帮助，寻找亲友的帮助，或寻找专业的帮助，让你信任的人陪着你一起去面对这些痛苦的事情。美国心理学家罗杰斯曾是最孤独的人，但当他面对这个事实并化解后，他成了真正的人际关系大师；美国心理学家弗兰克有一个暴虐而酗酒的继父和一个糟糕的母亲，但当他挑战这个事实并最终从心中原谅了父母后，他成了治疗这方面问题的专家；日本心理学家森田正马曾是严重的神经症患者，但他通过挑战这个事实并最终发明出了森田疗法……他们生命中最痛苦的事实最后都变成了他们最重要的财富。你，一样也可以做到。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "把一张纸折叠55次有多厚",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      想象一下，你手里有一张足够大的白纸。现在，你的任务是，把它折叠55次。那么，它有多高？一个冰箱？一层楼？或者一栋摩天大厦那么高？不是，差太多了，这个厚度超过了地球和太阳之间的距离。折叠55次的高度如此恐怖，但如果仅仅是将51张白纸叠在一起呢？只有5mm。这个对比让不少人感到震撼。
      
【温馨提示】

      看得见的力量比看不见的力量更有用。通过规划利用好现有的能力远比挖掘无目标的潜能更重要。没有方向、缺乏规划的人生，就像是将51张白纸简单叠在一起，今天做做这个，明天做做那个，每次努力之间并没有一个联系。这样一来，哪怕每个工作都做得非常出色，它们对你的整个人生来说也不过是简单的叠加而已。当然，人生比这个寓言更复杂一些。有些人，一生认定一个简单的方向而坚定地做下去，他们的人生最后达到了别人不可企及的高度。也有些人，他们的人生方向也很明确，譬如开公司，这样，他们就需要很多技能——专业技能、管理技能、沟通技能、决策技能等等。他们可能会在一开始尝试做做这个，又尝试做做那个，没有一样是特别精通的，但最后，开公司的这个方向将以前的这些看似零散的努力统合到一起，这也是一种复杂的人生折叠，而不是简单的叠加。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "龙卷风卷不走的雄心",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个夏末秋初的日子，露丝在西部大草原一所教会的晚餐会上，遇到一名达科他州的农夫。露丝担任国内传导团教派委员会全国议长，在教会的协议会上发表讲话。晚餐会上，她坐在那位农夫的对面，很自然地同他聊了起来。他口齿并不伶俐，显然很不习惯应酬。露丝想着不知他对什么样的话题感兴趣，
      她这样问道：“今年的收成还好吗？”
      “不好。事实上没有多少收获，我还算有一成的收成，我哥哥则完全没有。”
      露丝惊讶地问道：“这是为什么呢？”
      “是龙卷风作的孽。在十分钟之内把一切都卷走了。”说完他就沉默了。
      “你怎么面对那么严重的状态呢？”
      “你是问我怎么办吗？”他默默地想了一下说，“我只是努力忘记而已。”
      这个人多年来和大自然共同生活，和风、热、冷以及龙卷风共同生活，有过丰收，也碰到过欠收，他的工作就是守望着给我们提供一切的地球，这个优秀的农夫将龙卷风突然造成的灾害，视为人生的一个事实而平静地接受下来。他并没有想“要是那样的话”，也不哀怨地哭泣，而是决心下一次要获得更好的收获。不论在何种情形下他都会再站起来，继续前进，重新建造。他正是能称得上有积极态度的人。也就是有强大的雄心，积极面对一切困难的人。他知道这个世界上的善最后会超过恶。有一天终会发现解决问题的方法，一定会将困难克服。
      
【温馨提示】

      “如果”、“做不到”、“不可能”……这些消极的语言一起向自己冲过来时该怎么办呢？这时候就要以“下一次”、“可能”等积极的语言来迎击。这说来容易，实行起来却并不那么简单，往往困难重重。可是只要你不屈不挠地努力，积极思考，怀有信念，最后你一定能获得胜利，而且是巨大的胜利。还要学会如何做到达观，就会懂得看起来受到破坏的东西，还有其创造性的一面。而那样的一面，是由当初以为一切都被破坏的某种事件作为契机，真正归你所有的。事情进行不顺利时，最后还是有可能会有好的结果。因此当希望、梦想或目标被打碎时，就要在那残骸中仔细寻找，在看来已粉碎的残骸中，也许能找到最好的机会之苗。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "走不回来的人",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      从前，有一个人听说有一位部落首领能满足人们的需求，他很想要一块地，于是就去拜访这位部落首领。首领也很爽快说：“你从这儿向西走，做一个标记，只要你能在太阳落山之前走回来，从这儿到那个标记之间的地都是你的了。”
      此人听首领这么一说，简直乐坏了，他立刻行动。
      太阳落山了，这个人还没有走回来，因为他走得太远，累死在路上了。
      
【温馨提示】

      在人生的旅途中，每过一个时期，或每走一段路程，不妨回过头来看看自己的身后，看看在太阳落山之前自己是否还能走回去，或干脆停下来，沉思片刻，问一问：我要到哪里去？我去干什么？这样或许我们可以生活得更轻松些。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "少年雄鸡的职责",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一只雄鸡已经年老，当它奄奄一息快要离开这个世界的时候，它对守候在身旁的孩子说：“孩子，我已经不行了，从今以后，每天早晨呼唤太阳的职责，要由你来承担了。”少年雄鸡点点头，伤心地注视着慢慢闭上了眼睛的父亲。
      第二天一早，少年雄鸡飞上谷仓的屋顶。它朝东方，放开喉咙啼叫。但是，它发出来的却是一种缺乏力量的、时断时续的嘎嘎声。这天太阳没有升起，乌云布满天空，淅淅沥沥的毛毛细雨下个不停。饲养场上的所有动物都气坏了，跑来责怪少年雄鸡。
      “真是倒霉透了！”猪叫道。
      “我们需要阳光！”羊也叫起来。
      “雄鸡，你必须啼叫得更响一些！”公牛说，“太阳离我们有9300万英里远，你的叫声那么细小，它能听得见吗？”
      少年雄鸡被大家说得很难过，它决心一定要练好自己的本领，给家族争光。过了几天，少年雄鸡又一早就飞上谷仓的屋顶。它放开喉咙大声啼叫，这次发出的啼鸣声非常洪亮，在雄鸡啼鸣史上是空前的。
      “吵死人了！”猪说。
      “耳朵都要被震破了！”羊叫道。
      “头都要听炸了！”公牛抱怨说。
      “对不起，”少年雄鸡说，“但是我是在尽自己的职责。”
      它心里充满了自豪感，它看见了，在那遥远的东方，一轮红日正从丛林后面冉冉升起。
      
【温馨提示】

      促使成功的最大向导，就是人从自己的错误中总结出来的教训。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "马克?吐温抄菜单",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      马克?吐温是美国批判现实主义文学的奠基人，世界著名的短篇小说的大师。这位大师曾被人戏称为“笔记迷”，可见他做学问记笔记有多出名。
      有一次，马克?克温邀请一些朋友去饭馆吃饭，当大家都入座后，马克?吐温手里拿着一张菜单，大家都以为他准备点菜，只见他拿出笔和纸就抄了起来，抄了很多，大家以为他要点很多菜，吓了一跳，过一会儿大家才明白过来他在为自己抄菜单。朋友们见此很不高兴地问他：“你是请我们来吃饭的，还是让我们来陪你抄菜单的？”马克?吐温这时才意识到自己的失礼，他赶忙道歉，这才平息了朋友的怨气。他抄这些菜单有什么用呢？后来在他的一篇小说里便出现了这张菜单，使小说里的乡土气息被衬托得更加浓厚。马克?吐温曾经说过：“一个作家应该像画家一样，身上经常带着笔和纸，记录有意义的事情。”而他在现实生活中也真这么做了，这为他日后的创作提供了丰富的资料。
      
【温馨提示】

      读书学习必须勤于动笔，多做读书笔记，有利于积累总结有用的资料，提高文字表达能力；有利于训练思维的逻辑性、条理性。因此，我们每个人从学生时代起，就应该学会做笔记的方法，养成做笔记的习惯，这定会让我们受益终生。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "吃烧饼",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个人他肚子饿极了，就找到一家卖烧饼的铺子去买烧饼充饥。拿到烧饼时便狼吞虎咽地吃起来，吃了一个不饱，又买一个，再买一个，就这样他一连吃了8个，结果感觉还是不饱，于是他又买了第9个烧饼，只吃了一半便饱了。这时他开始后悔起来，懊丧地自责说：“唉，我怎么那么愚蠢啊，前面吃的8个烧饼都白白浪费了，早知道这半个烧饼就能吃饱的话，我就不用去买前面的8个烧饼了！”
      
【温馨提示】

      虽然是边际递减，但最后取得的成绩是之前一点一点积累起来的，只有不断地积累才会有这个结果。学习也一样，要的是平时一点一滴、扎扎实实的努力，踏踏实实地积累总结。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "十分钟",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个小孩喜欢玩电脑，他的母亲很恼火，认为会耽误学习，每每都要把电脑关掉。可孩子却好像入了迷，总是躲着大人玩，母亲连打带骂也不管用。后来，父亲看出孩子的潜质，决定换一种方式来疏导孩子，便在一旁撮和：“给你再玩十分钟。”母子俩的“战争”从此烟消云散。这个孩子后来不仅考上了一所名牌大学，而且还成为一名电脑奇才。短短的十分钟，在平实的心境中，使一个人的爱好和智慧得以挥洒和延伸，并最终有所成就，这是孩子的母亲始料不及的。
      一位演奏家偶然在一所普通的中学听到一个普通的语文老师弹奏《海边的阿迪丽亚》，发现其演奏水准丝毫不亚于专业音乐手，于是惊讶地问：“请问你熟悉这首曲子花了多少时间？”这位老师微笑道：“十分钟。”在专家疑惑的目光中她的一番解释让人感叹不已：“我们学校有一架钢琴，原来有一个音乐老师，后来她因故离开了学校，我就有机会来到琴房，每天利用课间十分钟来弹奏这首我心爱的曲子，从最初的间阶练起，才?????”
      
【温馨提示】

      世界上没有一蹴而就成功，它需要我们花费大量的时间来学习、钻研。只有善于不断地把时间分解整合并持之以恒的人，离成功才会越近。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "班超做事有弹性",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      班超是东汉著名的军事家和外交家，公元100年初，他担任西域都尉。班超在漠北任职达30多年，威慑西域诸国。在他任期内，西域各族不敢轻举妄动，因此汉朝西北部边疆及西域地区得以和安宁。为此朝廷封其为定远候，可谓成名就。当班超年老力衰时，便上表辞职。皇帝念其劳苦功高，便批准了他的请求，让任尚接替他的职务。在上任之前，任尚去拜访班超，想问问班超管理西域的妙方，任尚见到班超后问他：“我要上任去了，请您教我一些统治西域的方法。”班超了解任尚是个急性子，做事经常一板一眼的，于是对他说：“当水太清时，大鱼就没有地方躲藏，谅它们也不敢住下来；同样，为政之道也不能太严厉、太挑剔，否则也不容易成功。对西域各国未开化民族，不能太认真，做事要有弹性。大事化小繁事化简才是。”任尚听了，大不以为然。虽口头上表示赞成，内心却不服。“我本以为班超是个伟大人物，肯定有许多高招教我，却只说了些无关痛痒、无足轻重的话，真令我失望。”任尚果然把班超的教诲当作了耳旁风。他到达西域后，严刑厉法，一意孤行。结果没过多久，西域人便起兵闹事，该地就此失去了和平，又陷于激烈的刀兵状态。出现这样的结果，任尚想必是非常后悔的。但是，已酿成大乱，后悔也无济于事了。班超出使西域数十年，他的成功经验当然是宝贵的。任尚毫无治理西域的经验，应该认真领会才对。
      
【温馨提示】

      俗话说：“听君一席话，胜读十年书。”我们不应该太固执已见，看轻别人的经验，要多多参考长辈的建议，因为那是实践中总结出的经验之道，这样可以让我们少走许多弯路。总结是出奇制胜之道，总结是人的提高过程，总结是一条制胜之道，阶段总结是为了过程的胜利，这一次总结是为了下一次成功，只有认真总结才能取得持续进步，才能避免不愉快事情的发生，人生之路才会更顺畅。善于总结是一个优点，是一种诀窍，亦是种智慧。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "成功需要勇气",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一天，某公司总经理向全体员工宣布了一条纪律：'谁也不要走进8楼那个没挂门牌的房间。'但是，他没有解释为什么。此后真的没人违反他的这条'禁令'。
      三个月后，公司又招聘了一批员工。在全体员工大会上，总经理再次将上述'禁令'予以重申。这时，只听一个新来的年轻人在下面小声嘀咕了一句：'为什么?'总经理听到后并没有因这位新人的不礼貌而恼怒，只是满脸严肃地答道：'不为什么!'回到岗位上，那个年轻人百思不得其解，还在思考着总经理为什么要这样做。其他工友则劝他只管干好自己的那份差事，别的不用瞎操心。因为'听总经理的，总是没错'。可那个年轻人偏偏来了犟脾气，非要把事情弄个水落石出不可。于是他决定冒公司之大不韪，走进那个房间探个究竟。这天，他爬上8楼，轻轻地叩了叩那扇门，没有反应。年轻人不甘心，进而轻轻一推，虚掩着的门开了(原来门并没有上锁)。房间里没有任何摆设，只有一张桌子。年轻人来到桌旁，看到桌子上放着一个纸牌，上面用毛笔写着几个醒目的大字--'请把此牌送给总经理'。
      年轻人拿起那个已落满灰尘的纸牌，走出房间似有所悟，乘电梯直奔15楼总经理办公室。当他自信地把纸牌交到总经理手中时，仿佛期待已久的总经理一脸笑意地宣布了一项让年轻人感到震惊的任命:'从现在起，你被任命为销售部经理助理。'
      在后来的日子里，那个年轻人果然不负厚望，不断开拓进取，把销售部的工作搞得红红火火，并很快被提升为销售部经理。事后许久，总经理才向众人做了如下解释：'这位年轻人不为条条框框所束缚，敢于对上司的话问个'为什么'，并勇于冒着风险走进某些禁区'，这正是一个富有开拓精神的成功者应具备的良好素质。'
      
【温馨提示】

      其实，很多成功的门都是虚掩着的，只有勇敢地去叩开它，大胆地走进去，才能探寻出个究竟来。或许，那时呈现在你眼前的真的就是一片崭新的天地。毕竟，勇气是成功的前提。敢于破禁区者，必有意想不到的收获。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "自信的距离",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      他是杂技团的台柱子，凭借一出惊险的高空走钢丝而声名远扬。在离地五六米的钢丝上，他手持一根中间黑色、两端蓝白相间的长木杆作平衡，赤脚稳稳当当地走过10米长的钢丝。他技艺高超，身手灵活，还能从容地在钢丝上做出一些腾跃翻转的动作。多年来，他表演过无数次，从未有过丝毫闪失。
      杂技团去外地演出回来的路上，装道具的卡车翻进了山沟，折断了他那根保持平衡的长木杆。团里非常重视，不惜高价找来了粗细相同、长短一致、重量也一样的木杆。直到他觉得得心应手时，团长才请油漆匠给木杆刷上与以前那根木杆相同的蓝白相间的颜色。
      又是一次新的演出。在观众的阵阵掌声中，他微笑着赤脚踏上钢丝。助手递给他那根蓝白相间的长木杆。他从左端开始默数，数到第10个蓝块，左手握住，又从右端默数第10个蓝块，右手握紧，这是他最适宜的手握距离。然而今天，他感到两手间的距离比他以往的长度短了一些。他心里猛地一惊，难道是有人将木杆截短了？不可能啊？！他小心翼翼地把两手分别向左右移动，一直到适宜的距离才停住。他看了看，两手都偏离了蓝块的中间位置。他一下子对木杆产生了怀疑。
      这时，观众席上又一次爆发出雷鸣般的掌声，已经容不得他多想。他握紧木杆，提了一口气，向钢丝的中间走去。走了几步，他第一次没了自信，手心有汗沁出。终于，在钢丝中段做腾跃动作时，一个不留神，他从空中摔了下来，折断了踝骨，表演被迫停止。
      事后检查，那根木杆长度并没变，只是粗心的油漆匠将蓝白色块都增长了一毫米。
      
【温馨提示】

      　很多时候，我们的自信都是受习惯思维的影响，事物的表面现象左右着我们的固定思维，并不一定是事物的本质发生变化。木杆的长度没有变，但自信的距离改变了，就是这一毫米长度的变化，影响了他的成败。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "误会是把双刃剑",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      早年在美国阿拉斯加有一个年轻人结婚后太太因难产而死，遗下一孩子。他忙于生活，又忙于看家，因没有人帮忙看孩子，就训练一只狗，那狗聪明听话，能照顾小孩，咬着奶瓶喂奶给孩子喝，抚养孩子。有一天，主人出门去了，叫它照顾孩子。他到了别的乡村，因遇大雪，当日不能回来。第二天才赶回家，狗立即闻声出来迎接主人。他把房门开一看，到处是血，抬头一望，床上也是血，孩子不见了，狗在身边，满口也是血，主人发现这种情形，以为狗性发作，把孩子吃掉了，大怒之下，拿起刀把狗杀死了。然听到孩子的声音，又见他从床下爬了出来，於是抱起孩子；可发现孩子虽然身上有血，但并未受伤。他很奇怪，不知究竟是怎么一回事，再看看狗身，腿上的肉没有了，旁边有一只狼，口里还咬着狗的肉；狗救了小主人，却被主人误杀了，这真是天下最令人惊奇的误会。
      
【温馨提示】

      误会往往是在不了解、无理智、无耐心、缺少思考、未能多方体谅对方反省自己、感情极为冲动的情况之下所发生。人对无知的动物小狗发生误会，尚且会有如此可怕严重的后果，这样人与人之间的误会，则其后果更是难以想像。虽然误会是难免的，但不即时解除误会，它会象滚雪球样的越滚越大，到无法收拾的地步。误会是把双刃剑，发生误会的双方都会受到伤害，让我们把心胸放宽大些，冷静地对待误会，主动地解除误会吧。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "弥乐佛和韦陀佛",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      去过庙的人都知道，一进庙门，首先是弥陀佛，笑脸迎客，而在他的北面，则是黑口黑脸的韦陀。但相传在很久以前，他们并不在同一个庙里，而是分别掌管不同的庙。
      弥乐佛热情快乐，所以来的人非常多，但他什么都不在乎，丢三拉四，没有好好的管理账务，所以依然入不敷出。而韦陀虽然管账是一把好手，但成天阴着个脸，太过严肃，搞得人越来越少，最后香火断绝。
      佛祖在查香火的时候发现了这个问题，就将他们俩放在同一个庙里，由弥乐佛负责公关，笑迎八方客，于是香火大旺。而韦陀铁面无私，锱珠必较，则让他负责财务，严格把关。在两人的分工合作中，庙里一派欣欣向荣景象。
      
【温馨提示】

      世界上没有真正的垃圾，只是没有发现他真正的用途。人也是如此，世上没有无用的人，只要充分运用各自的长处，正如武功高手，不需名贵宝剑，摘花飞叶即可伤人，关键看如何运用。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "蜡烛的光辉",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      二战期间，英国小说家西雪尔?罗伯斯到伦敦郊外的一个墓地去祭奠一位英年早逝的文友，意外地看到一块新的墓碑，上面写道：“全世界的黑暗，也不能使一枝蜡烛失去光辉。”奇异的语言，温暖了罗伯斯阴郁的心。起初还以为一定是哪位名家的名言，可回来以后怎么也查找不到来源。还是重返墓地才从管理员那里得知，长眠于那个墓碑之下的是一位年仅10岁的少年，几天前，德军空袭伦敦时不幸被炸弹夺去了生命。少年的母亲怀着极度悲痛为自己的儿子做了一个墓，并立下了那块墓碑。
      这句话，让西雪尔?罗伯斯顿生无法释怀的激情，顷时，写出一篇感人至深的短文。短文迅速传播开来，鼓舞了一批反法西斯青年为胜利执著向前。
      这句话后来还让一名叫布雷克的大学毕业生，毅然放弃多家企业的优厚招聘，拒绝亲友的劝阻，去了非洲，在那里照出一片亮地，终被联合国授予“扶贫大使”称号。
      
【温馨提示】

      “全世界的黑暗，也不能使一枝蜡烛失去光辉。”这句话，不急不暴、不恐不怨、不浮不躁。一种正义的力量，一种由爱释放出来的母亲的力量，最强大也最平和。真正的强力，可以是这样。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "球王贝利的故事",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在里约热内卢的一个贫民窟里，有一个男孩，他非常喜欢足球，可是买不起，于是就踢塑料盒，踢汽水瓶，踢从垃圾箱里拣来的椰子壳。他在巷口里踢，在能找到的任何一片空地上踢。有一天，当他在一个干涸的水塘里猛踢一只猪膀胱时，被一位足球教练看见了，他发现这男孩踢得很是那么回事，就主动提出送他一只足球。小男孩得到足球后踢得更卖劲了，不久，他就能准确地把球踢进远处随意摆放的一只水桶里。
      圣诞节到了，男孩的妈妈说：“我们没有钱买圣诞节礼物，送给我们的恩人。就让我们为我们的恩人祈祷吧。”小男孩跟妈妈祷告完毕，向妈妈要了一只铲子，跑了出去，他来到一处别墅前的花园里，开始挖坑。就在他快挖好的时候，从别墅里走出一个人来，问小孩在干什么，小男孩抬起满是汗的脸蛋，说：“教练，圣诞节快乐，我没有礼物送给您，我愿给您的圣诞树挖一个树坑。”教练把小男孩从树坑里拉上来，说：“我今天得到了世界上最好的礼物。明天你就到我的训练场去吧。”
      三年后，这位十七岁的小男孩在第六届世界足球锦标赛上独进二十一球，为巴西第一次捧回金杯。一个原来不为世人所知的名字——贝利，随之传遍世界。
      
【温馨提示】

      感恩的广式有很多种，真心地付出总是有意想不到的回报。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "决定改变人生",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有谁曾想到当年印度圣哲甘地，一位温和、谦逊、主张和平的执业律师，凭着决定和胆识竟然能率领印度人挣脱大英帝国的统治，结果还引发其他殖民地一连串的独立，整个改变国际强权的均势。
      在古巴导弹危机期间，肯尼迪就是凭着决定，敢于和赫鲁晓夫对峙，结果避开了第三次世界大战的爆发。同样的情形，就是因为有坚强的决定，所以马丁路德?金博士才能侃侃而谈，道出美国黑人所遭受的非人待遇，激发起美国的民权运动，引起全球瞩目。
      “决定”是房地产大亨唐纳德特朗普彗星般崛起与坠落的主要原因，也是棒球全垒打明星贝比?鲁斯列入棒球名人堂的推动力量。
      一九五五年的某一天，罗莎?帕克斯在阿拉巴马州蒙哥马利市搭乘公车，理直气壮地不按该州法律规定让位给一位白人。她这个不服从的举动造成轩然大波，招来白人强烈的抨击，然而却也成为其他黑人效法的榜样，结果掀起了随后的民权运动，使美国人民的良知普遍觉醒，为平等、机会和正义重新界定出不分种族、信仰和性别的法律。谁能想到这个弱女子的决定，却给后人带来如此深远的影响？
      艾德是一个很“平凡”的人，十四岁时因感染小儿麻痹症而致颈部以下瘫痪，得靠轮椅才能行动，然而他却因此而有“不平凡”的成就。他使用一个呼吸设备，白天得以过正常人的生活，但晚上则有赖“铁肺”。得病之后他曾好几度几乎丧命，不过他可从不为自己的不幸伤心难过，反而自勉期能有朝一日帮助相同的患者。他决定教育社会大众，不要以高高在上的姿态认为肢体残疾的人无用，而应顾及他们生活中的不便处。在他过去十五年中的推动下，社会终于注意到了残疾人的权利，如今各个公共设施都设有轮椅专走的上下斜道，有残疾人专用的停车位，帮助残疾人行动的扶手，这都是艾德的功劳。艾德?罗伯茨是第一个患有颈部以下瘫痪而毕业于加州大学柏克莱分校的高材生，随后他又任职加州州政府复建部门的主管，也是第一位担任公职的严重残疾人士。
      肢体上的不便并不能限制一个人的发展，重要的是他是否决定要结束这样的不便。他的一切行动只不过源自于一个单纯但有力量的决定。艾德?罗伯茨不像其他的瘫痪者只看到自己的不幸，却没看到可以改善瘫痪者的生活世界；肢体上的不便，对他而言乃是能力与信念的考验；而他明显所要做的事，便是如何使其他的瘫痪者过着方便的生活。
      
【温馨提示】

      当你作出一个崭新、认真且不移的决定时，你的人生在那一刻便会改变。不知道怎么作决定并不重要，重要的是你要决心找出一个办法来，不管那是个什么样的办法：第一，决定出你所要追求的是什么；第二，拿出行动来；第三，观察一下哪个行动管用，哪个行动不管用；第四，如果行动方向有偏则修正之，以能达到目标为准。在任何时刻里，都有三个必须作决定的要素主宰着我们的人生，它决定了日后我们的成就，这三个要素分别是：一、你要决定怎么看；二、你要决定怎么想；三、你要决定怎么做。在这三个主宰人生的要素里并不包括你现在及过去的遭遇，而取决于你对于自己的一切是采取什么样的看法、想法及做法。如果你知道有哪个人比你有更大的成就，不管这个成就是哪一方面的，定然是他在这三个要素上有和你不同的决定。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "五枚金币",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有个叫阿巴格的人生活在内蒙古草原上。有一次，年少的阿巴格和他爸爸在草原上迷了路，阿巴格又累又怕，到最后快走不动了。爸爸就从兜掏出５枚硬币，把一枚硬币埋在草地，把其余４枚放在阿巴格的手上，说：“人生有５枚金币，童年、少年、青年、中年、老年各有一枚，你现在才用了一枚，就是埋在草地的那一枚，你不能把５枚都扔在草原，你要一点点地用，每一次都用出不同来，这样才不枉人生一世。今天我们一定要走出草原，你将来也一定要走出草原。世界很大，人活着，就要多走些地方，多看看，不要让你的金币没有用就扔掉。”在父亲的鼓励下，那天阿巴格走出潦草原。
      长大后，阿巴格离开了家乡，成了一名优秀的船长。
      
【温馨提示】

      珍惜生命，就能走出挫折的沼泽地。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "不要开错窗",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个小女孩趴在窗台上，看窗外的人正埋葬她心爱的小狗，不禁泪流满面，悲恸不已。她的外祖父见状，连忙引她到另一个窗口，让她欣赏他的玫瑰花园。果然小女孩的心情顿时明朗。老人托起外孙女的下巴说：“孩子，你开错了窗户。”
      
【温馨提示】

      打开失败旁边的窗户，也许你就看到了希望。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "富兰克林?罗斯福的故事",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个小男孩认为自己是世界上最不幸的孩子，脊髓灰质炎给他留下了一条瘸腿和一嘴参差不齐的牙齿。因此，他很少与同学们游戏和玩耍，老师叫他回答问题时，他也总是低着头一言不发。
      在一个平常的春天，小男孩的父亲从邻居家讨了些树苗，他想把它们栽在房前院子里。他叫孩子们每人栽一棵，父亲说，谁栽的树苗长得最好，就给谁买一件最好的礼物。小男孩也想得到父亲的礼物，但看到兄妹们蹦蹦跳跳提水浇树的身影，不知怎么地，他竟然萌生出这样一种想法：希望自己栽的那棵树早日死去。因此，浇过一两次水后，他就再也没有去答理它。
      几天后，小男孩再去看他种的那棵树时，惊奇地发现它不仅没有枯萎，而且还长出了几片新叶子，与兄妹们种的树相比，似乎更显得嫩绿，更有生气。父亲兑现了他的诺言，为小男孩买了一件他最喜爱的礼物。父亲对他说，从他栽的树来看，他长大后一定能成为一个出色的植物学家。从那以后，小男孩就对生活有了美好的憧憬，慢慢地变得乐观开朗起来。
      一天晚上，小男孩躺在床上睡不着，看着窗外明亮皎洁的月光，忽然想起生物老师曾说过的话：植物一般都在晚上生长。何不去看看自己种的那棵小树是不是在长高？当他轻手轻脚来到院子时，看见父亲正用勺子在给自己栽的树苗浇水。顿时，明白了，原来父亲一直在偷偷在护育着自己的那棵小树！他返回房间，禁不住泪流满面……
      几十年过去了，那个瘸腿的小男孩没有成为一个植物学家，但他却成了美国总统。他的名字叫富兰克林?罗斯福。
      
【温馨提示】

      爱是生命中最好的养料，哪怕只是一勺清水。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "童年的贝壳",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在夏威夷一座偏远的小岛上，一位老师对一群小学生解释，为何人们在圣诞节要互赠礼物。老师说：“礼物表示我们的关爱和对耶酥降临的欢喜，也表示对爱我们的人的感激之情。”
      圣诞节到了，一个男孩为老师带来一只闪闪发亮的贝壳，它是贝壳中少有的珍品。
      老师问：“你在哪儿发现这样一只不寻常的贝壳？”
      男孩告诉老师：“在二十多公里外的一个隐秘海滩，有时会有这种闪亮的贝壳被冲上岸。我爸爸说，那是一种很珍贵的贝壳。于是，我走了二十多公里，为老师捡来了这样一只贝壳。”
      老师说：“它真是太美了，我会一辈子珍惜它的。但你不应该走那么远的路专门去为我带一件礼物。”
      男孩眨着眼睛说：“一个贝壳作为礼物可能太轻了，我还把走路也作为礼物送给你。”
      老师一下子紧紧抱住小男孩，激动地说：“你的贝壳我很喜欢，但你‘走路’这份礼物我觉得更珍贵。”
      
【温馨提示】

      当我们童年时，我们会使一只贝壳作为礼物，还加上“走路”；当我们成为少年，我们的礼物会变成玫瑰，再加上海誓山盟；可是，当我们成人时，我们的礼物不幸变成了金钱和财宝；等我们年老，我们最好的礼物就只有回忆和问候──这时候，我们最想得到的，就是童年的一只小贝壳……",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "爱、成功、财富",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位夫人打开房门，看到三位银须飘然的老者坐在她家门前的台阶上。
      虽然夫人与他们素不相识，但还是礼貌地上前打招呼：“您们一定饿坏了，进屋去吃点东西吧。”
      “你家男主人在吗?”老人们问。
      “在呀。”夫人答道。
      “那你先去征求一下他的意见吧。”老人们回答。
      夫人回屋里将此事告诉了丈夫。丈夫说：“快去告诉他们，请他们进来吧!”于是她又出去邀请他们。
      “可是，我们不会一同进去。”老人们说。夫人感到疑惑。
      一个老人指着一个同伴说：“他名叫财富，”又指着另一个同伴说，“他叫成功，我是爱。”他接着说，“我们只进去一个人，你去和丈夫商量一下，看你们愿意让哪一位进去。”
      夫人把老人们的话告诉了丈夫，丈夫十分惊喜，说：“既然如此，我们就邀请财富老人吧，快去请他进来!”
      妻子不同意，说：“亲爱的，为什么不邀请成功呢?”
      这时他们的女儿插话了：“我想，邀请爱进来不是更好吗?一家人拥有爱是最好的。”
      “那就听女儿的话吧！”丈夫对妻子说。夫人出去告诉三位老人：“我们商量过了，请叫‘爱’的老人跟我来吧!”
      爱朝屋里走去，可是另外两位老人也跟在后面。夫人不解地问财富和成功：“刚才我邀请您们一起进来，您们说不能一同进屋。现在我邀请的是爱，您们怎么又愿意来了呢?”
      老人们一同回答说：“难道你们不知道吗?哪里有爱，哪里就有财富和成功!”
      
【温馨提示】

      财富和成功永远跟在爱的后面。",
            group2));

            group2.Items.Add(new SampleDataItem("Group-4-Item-3",
            "玻璃球的爱",
            "Item Subtitle: 3",
            "Assets/title/2.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在经济大萧条的时期，小蔬菜商米勒斯先生总是在路边摆一个小菜摊，镇上的人办完事回家时，就顺便到这里采购一些新鲜的蔬菜。当时食品和钱都极度紧缺，物物交换就被广泛采用了。
      在镇上，有几个家里很穷的孩子，他们经常光顾米勒斯先生的菜摊。不过，他们似乎并不想购买什么东西，只是来欣赏那些在当时非常珍贵的物品。尽管如此，米勒斯先生总是热情地接待他们，就像对待每一个来买菜的大人一样。
      “你好，巴里！今天还好吧？”
      “你好，米勒斯先生。我很好，谢谢。这些豌豆看起来真不错。”
      “可不是嘛。巴里，你妈妈身体怎么样？”
      “还好。一直在好转。”
      “那就好。你想要点什么吗？”
      “不，先生。我觉得你的那些豌豆真新鲜呀！”
      “你要带点回家吗？”
      “不，先生。我没钱买。”
      “你有什么东西和我交换吧？用东西交换也可以呀！”
      “哦……我只有几颗赢来的玻璃球。”
      “真的吗？让我看看。”
      “给，你看。这是最好的。”
      “看得出来。嗯，只不过这是个蓝色的，我想要个红色的。
      “你家里有红色的吗？”
      “差不多有吧！”
      “这样，你先把这袋豌豆带回家，下次来的时候让我看看那个红色玻璃球。”
      “一定。谢谢你，米勒斯先生。”
      每次米勒斯先生和这些小顾客交谈时，米勒斯太太就会默默地站在一旁，面带微笑地看着他们谈判。她熟悉这种游戏，也理解丈夫所做的一切。
      镇上还有两个像巴里一样的小男孩，这三个孩子的家境都非常不好，他们没有钱买菜，也没有任何值钱的东西可以交换。为了帮助他们，又显得非常自然，米勒斯就这样假装着和他们为一个玻璃球讨价还价。就像巴里，这次他有一个蓝色的玻璃球，可是米勒斯先生想要红色的；下次他一准儿会带着红玻璃球来，到时候米勒斯又会让他再换个绿的或桔红的来。当然打发他回家的时候，一定会让他捎上一袋子上好的蔬菜。
      米勒斯先生因病过世时，镇上所有的人都去向他的遗体告别，并向米勒斯太太表示慰问，包括那些年幼的孩子。在长长的告别队伍前面，有三个头戴礼帽，身着笔挺的黑西服白衬衫非常引人注目的小伙子，逐一拥抱米勒斯太太，亲吻她的面颊，和她小声地说几句话。然后，她泪眼蒙蒙地目视他们在灵柩前停留，看着他们把自己温暖的手放在米勒斯先生冰冷苍白的手上。这三个小伙子就是当年经常用玻璃球之类的小玩艺儿和米勒斯先生交换蔬菜食品的那几个穷孩子。在同米勒斯太太握手慰问的时候，他们告诉她，他们多么感激米勒斯先生，感谢他当年“换给”他们的东西。现在，米勒斯先生再也不会再对玻璃球的颜色和大小改变主意了，这三个孩子也再不需要他接济度日，但是，他们永远都不会忘记他。虽然米勒斯先生一生从没发过大财，可是现在，他完全有理由认为，自己是爱达荷州最富有的人。在他已经失去生命的右手里，正握着三颗晶莹闪亮的红色玻璃球。
      
【温馨提示】

      同情心是可贵的，但同情常常会不自觉地演变为对自我的炫耀和对他人的可怜。如果是这样，同情已不是同情，同情就变成了虚荣和轻视。付出了同情又不流露，这是平常人难以做到的。米勒斯先生做到了，因为他付出的不仅是同情，还有爱。",
            group2));



            this.AllGroups.Add(group2);
            #endregion

            #region 第3章
            var group3 = new SampleDataGroup("Group-3",
            "第3章",
            "Group Subtitle: 3",
            "Assets/title/3.png",
            "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
"天堂在何方",
"Item Subtitle: 3",
"Assets/title/3.png",
"Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
@"
      一个人历尽艰险找天堂，终于被他找到了。当他欣喜若狂地站在天堂门口欢呼：“我来到天堂了！”天堂大门的守门人诧异的问：“你说这里就是天堂？”欢呼者顿时傻了：“你难道不知道这里就是天堂？”守门人茫然摇头问道：“你从哪里来？”“地狱。”守门人仍是茫然。欢呼者慨然嗟叹：“你不知道天堂何在，是因为你没有去过地狱。”
      
【温馨提示】

      你若渴了，水便是天堂；你若累了，床便是天堂；你若失败了，成功便是天堂；你若痛苦了，幸福便是天堂。你若没有拥有过其中的一样，你断然不会拥有另外一样的。天堂是地狱的终极，地狱是天堂的走廊。当你手中捧着一把沙子时，请千万不要把它丢弃，因为金子就在其中蕴藏。",
group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "学会拒绝，学会说NO",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一位名叫罗斯恰二斯的犹太人，于耶路撒冷开了一家名为“芬克斯”的酒吧。酒吧的面积不大，30平方米，但它却声名远播。
      有一天，他接到了一个电话，那人用十分委婉的口气和他商量说：“我有十个随从，他们将和我一起前往你的酒吧。为了方便，你能谢绝其他顾客吗？”罗斯恰尔毫不犹豫地说：“我欢迎你们来，但要谢绝其他顾客，不可能。”打电话的不是别人，是美国国务卿基辛格博士。他是访问中东的议程即将结束时，别人的推荐下，才打算到“芬克斯”酒吧的。基辛格后来坦言告诉他：“我是出访中东的美国国务卿，我希望你能考虑一下我的要求。”罗斯恰尔礼貌地对他说：“先生，您愿意光临本店我深感荣幸，但是，因您的缘故而将其他人拒于门外，我无论如何办不到。”基辛格博士听后，摔掉了手上的电话。
      第二天傍晚，罗斯恰尔又接到了基辛格博士的电话。首先他对前面的失礼表示歉意，说明天打算带三个人来，订一桌，并且不必谢绝其他客人。罗斯恰尔斯说：“非常感谢您，但是我还是我无法满足您的要求。”基辛格很以外，问：“为什么？”“对不起，先生，明天是星期六，本店休息。”“可是，后天我就要回美国了，您能否破例一次呢？”罗斯恰尔斯很诚恳地说：“不行，我是犹太人，您该明白，礼拜六是个神圣的日子，如果经营，那是对神的玷污。”
      此小酒吧连续多年被美国《新闻周刊》列入世界最佳酒吧前十五名。在罗斯恰尔斯的身上体现了一种十分珍贵的品质，那就是：拒绝的勇气。在需要拒绝的时候，他勇于拒绝任何人──包括基辛格那样的高官和权贵。
      
【温馨提示】

      拒绝是一门最棘手的艺术。它经常被认为是一种不善的行为，其实，拒绝有时候恰恰是一种美德。只有那些能够在适当时候拒绝一些东西的人，生活才能过得洒脱自尊。学会拒绝，学会说NO。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "给比尔?盖茨的小费",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      第一次走出乡村的米莎太太，拖着两个很大的行李箱，走进了候机大厅。环顾四周，寻觅了半天，也没有找到说好要来接她的侄子。她轻叹了一口气，只好坐下来等候。因为刚刚做过肾脏手术，米莎太太一直要频繁地上厕所，可是她又不也丢下行李箱不管。她带的许多东西虽然不很值钱，但却很珍贵，因为那是她给远在都市里的亲友们积攒了多年的礼物。她只得一边忍耐着，一边焦急地东张西望，盼着侄子早点出现。
      “太太，需要帮忙吗？”一个坐在旁边候机的年轻人，面带微笑地问她。
      “哦，不，暂时不需要。”米莎打量了年轻人一眼。
      身着休闲服的年轻人掏出一本书，专心致志地阅读起来。
      “这个不守时的家伙，等会儿非得训斥他不可。”米莎太太开始埋怨起来。
      又过了一会儿，米莎太太实在忍不住了，她向身旁的年轻人恳求道：“请帮我照看一下行李，我去一趟洗手间。”年轻人非常愉快地点了点头。米莎太太很快回来了，她感激地掏出一美元，递给年轻人：“谢谢你帮我照看东西，这是你应得的报酬。”
      望着老人一脸的认真，年轻人回一声“谢谢”，接过钱放进了衣兜。这时，米莎太太的侄子快步从门口走了进来，他刚要解释迟到的原因，忽然惊喜地冲着老人身旁的年轻人叫道：“你好，盖茨先生。没想到你会在这里候机！”
      “哦，是的。我的工作需要我经常到处跑。”年轻人收起书，准备去检票口检票。
      “哪个盖茨？”米莎太太不解地追问道。
      “就是我常常跟您说起的世界首富，微软公司总裁比尔?盖茨先生啊！”
      “啊，我刚才还给过他一美元的小费呢。”米莎太太满脸自豪地说。
      “他真的接受了你一美元的小费吗？”侄子惊讶地张大了嘴巴。
      “没错，我很高兴今天在候机的时候还有一美元的收入，因为我帮助这位太太做了一件很小的事。”盖茨回头坦然地答道。
      
【温馨提示】

      一美元是微不足道的，但在这里却表现出了金钱最纯正的品质：在清贫的乡村老妇米莎太太眼里，那是对一种劳动必须支付的报酬；而对于身家数百亿美元的世界首富盖茨来说，接受这一美元，是对一份真诚谢意的礼貌回应和尊重。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "沉默未必是金",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个神父很苦恼，事情的起因是由于一个男人在他面前作过一次忏悔。“实话相告，我是个杀人犯。”那男人坦白说，他是一起杀人案中真正的凶手，而该案的嫌疑犯已被逮捕并判处死刑。神父本应该向警察局报告这件事的真相，可是他的教规严禁将忏悔者的秘密泄漏他人。他不知如何是好。如果就这样保持沉默，一个无辜的人即将冤死，这会使他的良心不安。但是要打破教规，这对于发誓将一生献给上帝的他来说，无论如何也做不到。他陷入了进退两难之中。最后，他决定保持沉默。于是，他来到另一个神父的面前忏悔。“我将眼看着一个无辜的人被处死……”他陈述了事情的来龙去脉。这位神父朋友也为难了。想来想去，他也决定保持沉默。为了逃避良心的谴，他又向另外一个神父忏悔……在刑场上，神父问死囚：“你还有什么要说的吗？”“我没有罪，我冤枉！”死囚叫道。“这我知道。”神父回答，“你是无辜的，全国的神父都知道。但是，我们有什么办法呢？”
      
【温馨提示】

      每一个人一生中都见证过无数真相，见证过无数丑闻，但因为这些事与自己无关，或者与自己有关同时也关系他人，为了明哲保身免担风险，就选择沉默。古人云：沉默是金。当不该沉默的时候保持沉默，沉默就变成推卸责任的方式，就如故事中的神父，这样的神父多了，人类的良知就沦丧了——沉默未必是金。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "你的财富就在哪里",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一位昆虫学家和他的商人朋友一起在公园里散步、聊天。忽然，他停住了脚步，好像听到了什么。
      “怎么啦？”他的商人朋友问他。
      昆虫学家惊喜地叫了起来：“听到了吗？一只蟋蟀的鸣叫，而且绝对是一只上品的大蟋蟀。”
      商人朋友很费劲地侧着耳朵听了好久，无可奈何地回答：“我什么也没听到！”
      “你等着。”昆虫学家一边说，一边向附近的树林小跑了过去。不久，他便找到了一只大个头的蟋蟀，回来告诉他的朋友：“看见没有？一只白牙紫金大翅蟋蟀，这可是一只大将级的蟋蟀哟！怎么样，我没有听错吧？”
      “是的，您没有听错。”商人莫名其妙地问昆虫学家：“您不仅听出了蟋蟀的鸣叫，而且听出了蟋蟀的品种──可您是怎么听出来的呢？”
      昆虫学家回答：“个头大的蟋蟀叫声缓慢，有时几个小时就叫两三声。小蟋蟀叫声频率快，叫得也勤。黑色、紫色、红色、黄色等各种颜色的蟋蟀叫声都各不相同，比如，黄蟋蟀的鸣叫声里带有金属声。所有鸣叫声只有极其细微，甚至言语难以形容的差别，你必须用心才能分辨得出来。”
      他们一边说，一边离开了公园，走在马路边热闹的人行道上。忽然，商人也停住了脚步，弯腰拾起一枚掉在地上的硬币。而昆虫学家依然大踏步地向前走着，丝毫没有听见硬币的落地之声。
      昆虫学家的心在虫子们那里，所以他听得见蟋蟀的鸣叫。商人的心在钱那里，所以，他听得见硬币的响声。
      
【温馨提示】

      每个人对财富的定位是不同的，你的心在哪里，你的财富就在哪里。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "被人相信是一种幸福",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      从前，有一艘货轮在烟波浩淼的大西洋上行驶。一个在船尾做勤杂工的黑人小孩不慎掉进了波涛滚滚的大西洋里。孩子大喊救命，无奈风大浪急，船上的人谁也没有听见，他眼睁睁地看着货轮托着浪花越来越远……求生的本能使孩子在冷冰的水里拼命地游，他用全身的力气挥动着瘦小的双臂，努力使头伸出水面，睁大眼睛盯着轮船远去的方向。船越来越远，船身越来越小，到后来，什么都看见了，只剩下一望无际的汪洋。孩子力气也快用完了，实在游不动了，他觉得自己要沉下去了。放弃吧他对自己说。这时候，他想起了老船长那张慈祥的脸和友善的眼神。不，船长知道我掉进海里后，一定会来救我的！想到这里，孩子鼓足勇气用生命的最后力量又朝前游去……船长终于发现那个黑人孩子失踪了，当他断定孩子是掉进海里后，下令返航，回去找。这时，有人规劝：“这么长时间了，就是没有被淹死，也
      让鲨鱼吃了……”船长犹豫了一下，还是决定回去找。又有人说：“为一个黑奴孩子，值得吗？”船长大喝一声：“住嘴！”终于，在那孩子就要沉下去的最后一刻，船长赶到了，救起了孩子。当孩子苏醒过来之后，跪在地上感谢船长的救命之恩时，船长扶起孩子问：“孩子，你怎么能坚持这么长时间？”孩子回答：“我知道你会来救我的，一定会的！”“怎么知道我一定会来救你的？”“因为我知道您是那样的人！”听到这里，白发苍苍的船长扑通一声跪在黑人孩子面前，泪流满面说：“孩子，不是我救了你，而是你救了我啊！我为我在那一刻的犹豫而耻辱……”
      
【温馨提示】

      一个人能被他人相信也是一种幸福。他人在绝望时想起你，相信你会给予拯救更是一种幸福。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "我们的心就像是一座寺庙",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个皇帝想要整修在京城里的一座寺庙，他派人去找技艺高超的设计师，希望能够将寺庙整修成美丽而又庄严。后来有两组人员被找来了，其中一组是京城里很有名的工匠与画师，另外一组是几个和尚。由于皇帝没有办法决定到底哪一组人员的手艺比较好，于是他就决定要给他们机会作一个比较。皇帝要求这两组人员，各自去整修一个小寺庙，而这两个寺庙互相面对面；三天之后，皇帝要来验收成果。
      工匠们向皇帝要了一百多种颜色的颜料（漆），又要求了很多的工具；而让皇帝很奇怪的是，和尚们居然只要了一些抹布与水桶等等简单的清洁用具。三天之后，皇帝来验收两组人员装修寺庙的结果，他首先看看工匠们所装饰的寺庙，他们用了非常多的颜料，以非常精巧的手艺把寺庙装饰得五颜六色，皇帝很满意地点点头，接着回过头来看看和尚们负责整修的寺庙，他一看之下就愣住了，和尚们所整修的寺庙没有涂上任何的颜料，他们只是把所有的墙壁、桌椅、窗户等等都擦拭的非常干净，寺庙中所有的物品都显出了它们原来的颜色，而它们光泽的表面就像镜子一般，无瑕的反射出从外面来的色彩，那天边多变的云彩、随风摇曳的树影，甚至是对面五颜六色的寺庙，都变成了这个寺庙美丽色彩的一部份，而这座寺庙只是宁静地接受这一切。皇帝被这庄严的寺庙深深地感动了，当然我们也知道最后的胜负了。
      
【温馨提示】

      我们的心就像是一座寺庙，我们不需要用各种精巧的装饰来美化我们的心灵，我们需要的只是让内在原有的美无瑕的显现出来。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "苹果的功效",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      经济大恐慌期间，日本有许多中小企业都受到严重波及，纷纷面临破产的命运。有一家酱菜店也深受影响，老板虽然勉强经营，却是举步维艰。他很不甘心，心想：“努力了大半辈子，不能就这么放弃。”于是，他开始拼命研究如何让日益挑剔的客人们，愿意继续照顾他的生意。
      有一天，他想到一个绝妙的方法。老板跑到苹果的产地，订购了一批苹果，并在尚未成熟的苹果上贴了一个标签，等红苹果成熟时撕下标签纸，于是在水果上便会留下一片空白，老板便在这个空白处发挥创意呢！当周边的酱菜店纷纷倒闭，只有这家酱菜店的销售量大增，每天都顾客盈门，最后还扩大生产，供不应求。
      那位酱菜店老板的成功秘诀究竟是什么？就是苹果身上的创意，原来每次他都会从客户名单中挑出二百名客人，把他们的名字写在透明的标签纸上，并贴在苹果的空白处，然后随货品一起送给客人。每当客户收到这些苹果时，都充满惊奇与感动，他们没有想到老板竟
      如此细心与用心。酱菜店老板的这个小动作，其实花费不多，但是当每位客人接到这份小礼物时，心中都非常感动，更甚于赠送一箱酱菜的价值。因为这富有人情味的苹果，使得客户们深刻记住了这间酱菜店，只要苹果的盛产期一到，主顾们便会想起：“该是订购酱菜的时候了。”
      别小看这些苹果的功效，这不仅是老板的创意与关怀，更是他经商智慧的高度展现。所谓消费心理，其实就是让顾客有亲切感，当我们与销售员交流的时候，销售员的态度正是我们最在意的第一件事；态度亲切，懂得投顾客所好，业绩自然能蒸蒸日上。
      
【温馨提示】

      在人际交往过程，如果你希望拥有更多的“老顾客”，便要用心观察他们的需求，真心诚意地与他们交往，彼此就会充满关心与爱心，而不会有冲突和矛盾发生。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "跳高冠军——跳蚤",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在昆虫中，跳蚤可能是最善跳的了，他可以跳到自己身高的几万倍的高度。为什么会这样呢？带着这个问题，一个大学教授开始了他的研究。可是他研究了一整天，都没有找到答案。
      第一天下班的时候，教授用一个高一米的玻璃罩罩着这只跳蚤以防它逃跑。就在那天在晚上，跳蚤为了能跳出玻璃罩，就跳啊跳啊，可是无论它怎样努力，无论他怎么跳，都在跳到一米高的时候，就被玻璃罩挡了下来。
      第二天，教授上班取下玻璃罩，惊奇地发现，这只跳蚤只能跳一米高了。于是他来了兴趣。第二天下班时，教授用了一个50厘米的玻璃罩罩着跳蚤。
      第三天，教授发现跳蚤只能跳50厘米的高度；晚上，教授又用20厘米的玻璃罩罩着跳蚤。
      第四天，跳蚤跳的高度又降为20厘米。到了第四天下班时，教授干脆用一块玻璃板压着跳蚤，只让跳蚤能在玻璃板下面爬行。
      果然，到了第五天，跳蚤再也不能跳了，只能在桌面上爬行。可就在这个时候，教授不小心，打翻了桌上的酒精灯，酒精洒在了桌上，火也慢慢地向跳蚤爬的地方漫延。奇迹出现了，就在火快要烧着跳蚤的一瞬间，跳蚤又猛地一跳，又跳到了他最开始的超过他身体一万倍的高度。
      
【温馨提示】

      人的潜力就像这跳蚤的弹跳力一样，发挥出来时也是惊人的。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "要让别人看到你在努力",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      曾经有一个少年，到摩天大楼的工地向衣着华丽的承包商请教：“我应该怎么做，长大后才能和你一样有钱？”承包商看了少年一眼，对他说：“我跟你说一个故事：有三个工人在同一工地工作，三个人都一样努力，只不过，其中一个人始终没有穿工地发的蓝制服。最后第一个工人现在成了工头，第二个工人已经退休,而第三个没穿工地制服的工人则成了建筑公司的老板。年轻人，你明白这个故事的意义么？”少年满脸困惑，听的一头雾水，于是承包商继续指着前面那批正在鹰架上工作的工人对男孩说：“看到那些人了么？他们全都是我的工人。但是，那么多的人，我根本没法记住每一个人的名字，有些甚至连长相都没印象。但是，你看他们之中那个穿着红色衬衫的人，他不但比别人更卖力，而且每天最早上班，最晚下班，加上他那件红衬衫，使他在这群工人中显得
      特别突出。我现在就要过去找他，升他当监工。年轻人，我就是这样成功的，我除了卖力工作，表现得比其他人更好之外，我还懂得如何让别人“看”到我在努力。”
      
【温馨提示】

      不要以为只有你一个人在拼命工作，其实每个人都很努力！因此，如果想要在一群努力的人中脱颖而出，除了比别人做得更好之外，就靠其它得技巧和方法。最好的办法，就是找出自己与众不同的特质，将你的努力用在发挥这些特质上，如此一来，即使做的是相同的工作，那么你也会比别人更耀眼，更有可能获得成功的机会。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "要充分肯定自我",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1960年，哈佛大学的罗森塔尔博士曾在加州一所学校做过一个著名的实验。
      新学年开始时，罗森塔尔博士让校长把三位教师叫进办公室，对他们说：“根据你们过去的教学表现，你们是本校最优秀的老师。因此，我们特意挑选了100名全校最聪明的学生组成三个班让你们教，这些学生的智商比其他孩子都高，希望你们能让他们取得更好的成绩。”三位老师都高兴地表示一定尽力。校长又叮嘱他们，对待这些孩子，要像平常一样，不要让孩子或孩子的家长知道他们是被特意挑选出来的，老师们都答应了。
      一年之后，这三个班的学生成绩果然排在整个学区的前列。这时，校长告诉了老师们真相：这些学生并不是刻意选出的最优秀的学生，只不过是随机抽调的最普通的学生。老师们没想到会是这样，都认为自己的教学水平确实高。这时校长又告诉了他们另一个真相，那就是，他们也不是被特意挑选出的全校最优秀的教师，也不过是随机抽调的普通老师罢了。
      
【温馨提示】

      在做任何事情以前，如果能够充分肯定自我，就等于已经成功了一半。自信心就是成功的催化剂。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "明天的树叶不会今天落下",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个小男孩家门前有几棵大树。秋天起风的时候，树上的叶子就随风飞到院子里。于是，父亲就交给他一项任务，要他每天上学前将树叶打扫干净。对他来说，天刚亮就起床打扫树叶实在是一件苦差事。秋冬之际，树叶好像互相约好了似的，总是不停地落下来。头一天扫完了，第二天照常落满庭院；刚刚清扫完，一会儿又落下几片，似乎总是扫不净。后来，男孩从别人那里得到一个好主意：扫地之前，先将树使劲儿摇晃，这样就可以将第二天的树叶也摇落下来。这个主意令男孩兴奋不已，于是他起了个大早，扫地之前使劲儿将院外的树一棵棵摇了又摇。男孩累得满头大汗，这才发现摇树比扫地还累。但他毕竟做了一件让自己满意的事，那一天他非常开心。
      第二天，他高高兴兴地起床。谁知开门一看，院子里依然是落叶满地。男孩傻了眼，可是还是不死心，又去抱着树摇了又摇。但无论今天怎样用力，到明天清晨，还是会看到满地的黄叶。父亲知道男孩的烦恼，他没有责怪儿子。他告诉他，每天都会有落叶，今天只落今天的树叶，明天的树叶只能在明天落下。男孩站在满地落叶中，看着慈祥的父亲，突然大彻大悟。
      
【温馨提示】

      万事都不可急于求成，做好今天的事情，就是对一生负责。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "寻找鲜花的三只毛毛虫",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      小溪的树丛里，有三只毛毛虫，它们是从很远的地方爬来的。现在它们准备渡过小溪，到一个开满鲜花的地方去。
      甲说：“我们必须先找到桥，然后从桥上爬过去。只有这样，我们才能抢在别人的前头，找到含蜜最多的花朵。”
      乙说：“在这荒郊野外，哪里有桥？我们还是各造一条小船，从水上漂过去，只有这样，我们才能尽快到达对岸。”
      丁说：“我们走了那么多的路，已经疲惫不堪了，现在应该静下来休息两天。到时候，也许自然就有办法了。”
      甲和乙两个很诧异：“休息？简直是笑话！没看到对岸花丛中的蜜都快被喝光了吗？我们一路风风火火，马不停蹄，难道是来这儿睡觉的？”话未说完，甲则爬上河堤上的一条小路，它要寻找一座能过小溪的桥；乙已开始爬树，它准备折一片树叶做成船，让它把自己带过小溪去；剩下的丁则躺在树阴下没有动。它想，畅饮花蜜当然舒服，但这儿的习习凉风也该尽情享受一番。于是，就钻进一片树林，找了一片宽大的叶子，躺了下来。小溪的流水声如音乐一般动听，树叶在微风中如婴儿的摇篮，它很快就睡着了。
      不知过了多少时辰，也不知自己在睡梦中到底做了些什么，总之．一觉醒来，它发现自己变成了一只美丽的蝴蝶。它的翅膀是那样美丽，那样轻盈，轻轻煽动了几下，就飞过了小溪。此时，这儿的花开得正艳，每个花苞里都是香甜的蜜汁。它很想找到两个伙伴，可是，飞遍所有的花丛都没找到──因为它的伙伴一个累死在路上，另一个被溪水冲走了。
      
【温馨提示】

      在这个世界上，没有什么比顺其自然更具有力量；没有什么比顺乎本性更富有智慧。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "拥有自己的“自信罐”",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有个叫西格的女人，自从接连生了三个孩子之后，整天烦躁不安。4岁的孩子整日吵闹，19个月大的孩子整夜哭叫，还有一个婴儿需要不断地喂奶……那一段日子，西格的精神简直快要崩溃了。长期的睡眠不足，使她无法以正常的心态看待周围的世界，也无法正常地看待自己。她甚至怀疑自己天生就“低能”——连几个孩子都照看不好，以后还能做什么呢？就在这时候，一个叫海伦的朋友托人给她带了一份礼物。她打开一看，是一个装饰得很漂亮的陶瓷容器，上面还贴着一个标签，上面写着：“西格的自信罐，需要时用。”罐子里装着几十个用浅蓝色纸条卷成的小纸卷，每个小纸卷上都写着送给海伦的一句话。西伦迫不及待地一个个打开，只见上面分别写着：
      上帝微笑着送给我一件宝贵的礼物，她的名字叫“西格”；
      我珍惜你的友谊；
      我欣赏你的执着，还有你的热情；
      我希望住在离你的厨房100英尺远的地方；
      你很好客，而且贤惠能干；
      你有宽广的胸怀和金色美丽的长发；
      你是我最愿意陪伴着一起在超级市场转上一整天的那个人；
      你做什么事都那么仔细，那么任劳任怨；
      我真的相信你能做好任何你想做的事情；
      我给你提出两点建议：
      第一，当你完成一件自己想干的事情，或者得到别人的称赞和肯定的时候，就写一张小纸条放在这个罐里；
      因为她真切地感到，她正被别人爱着，被别人关心着。困难只是暂时的，自己仍然还是一个很棒的女人。从那以后，西格把这个“自信罐”摆在最醒目的地方，只要遇到危险和困难，就情不自禁地伸手去摸。15年以后，西格当了一所幼儿园的园长，很多家长都愿意把家长送到她这家幼儿园，因为她的自信激发了孩子们的自信。从这所幼儿园走出去的孩子，每个人都有一个“自信罐”。
      
【温馨提示】

      任何人到这个世界上，都拥有别人所不能拥有的东西。每一个人就如同树上的叶子，有着自己的独特性，任何人也取代不了你。你所需要的，只是找准你的人生方向。只要自己的“人生密码”和“事业密码”对上号，就像一把钥匙打开了一把锁，接着徐徐开启的，便是成功的大门。总有一种事业，总有一样东西，会让你大放异彩、出类拔萃。只是有很多人，在寻找的途中，因为困难、压力、气馁、甚至因为莫须有的恐惧，便轻言放弃。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "炮弹打偏了",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      第一次世界大战的凡尔登会战后期，炮火成了左右战局的重要力量。德军依仗其多年储备的众多大口径火炮狂施淫威，而法军战备不足，炮火虚弱，处于劣势。1916年4月，双方炮击两天两夜后的一天，位于马斯河上游的法军某炮兵阵地弹药所剩无几，炮兵伤亡过半。不得已，指挥官只好起用一批毫无开炮经验的后勤人员临时上炮位顶阵。其中有位年轻的下士因为对开炮怀有与生俱来的恐惧，在没有瞄准的情况下，手忙脚乱中将一发炮弹打了出去。炮弹一出膛，这位胆小的下士就失声叫道：“我的炮弹打偏了！”这发炮弹真是偏得太离谱了，德军阵地在东北方向，而炮弹飞向了西北方向。在弹药将尽之际。这种行为绝对是不可原谅的。指挥官挥舞着指挥棒气急败坏地向下士冲过来，准备狠狠教训他一顿。这时，只听见炮弹飞去的方向传来一声沉闷的爆炸，接着是巨大的连片爆炸声，炒豆似的绵延不绝，持续时间达30多分钟。所有的人都愣在那里，包括年轻的下士和指挥官，他们不知道究竟发生了什么事情。原来，这发打偏的炮弹鬼使神差地偏到了斯潘库尔森林中一座重要的德军秘密弹药补给基地，它成功地穿过狭窄的通风口直捣弹药库，引爆了基地所储备的全部弹药。这发炮弹造成了第一次世界大战中最大的一次爆炸，德军60多万发大口径炮弹和其他数十吨弹药销毁得一千二净，连一发臭弹都没给德意志帝国留下。于是，德军阵地上无数门大炮张着饥饿的大嘴成了一堆废铁。此前还焦头烂额的法军元帅贝当喜出望外，抓住时机大举反攻丧失了炮火支援的德军阵地。于是，凡尔登会战以能征善战的德军失败而载人史册，并进而决定了第一次世界大战的最后结局。
      时至今日，人们已无法确知当年的法国人如何评价这位胆小无能的法军下士和他那发偏离了预定目标的炮弹，但这个离奇却真实的故事会永远被人们记住。
      
【温馨提示】

      有时候，命运的炮弹也会发生类似的偏移，所以，如果你把某件事万一做砸了，不必过于沮丧，也许片刻之后就会听见出乎意料却令人惊喜的声响。不论你信不信，有时连错误都站在正义的一边──也许这一切都是上帝的创意。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "眼睛也会欺骗自己",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      两个旅行中的天使到一个富有的家庭借宿。这家人对他们并不友好，并且拒绝让他们在舒适的客人卧室过夜，而是在冰冷的地下室给他们找了一个角落。当他们铺床时，较老的天使发现墙上有一个洞，就顺手把它修补好了。年轻的天使问为什么，老天使回答：“有些事并不像它看上去那样。”第二晚，两人又到了一个非常贫穷的农家借宿。主人夫妇俩对他们非常热情，把仅有的一点点食物拿出来款待客人，然后又让出自己的床铺给两个天使。第二天一早，两个天使发现农夫和他的妻子在哭泣，他们唯一的生活来源——一头奶牛死了。年轻的天使非常愤怒，他质问老天使为什么会这样，第一个家庭什么都有，老天使还帮助他们修补墙洞，第二个家庭尽管如此贫穷还是热情款待客人，而老天使却没有阻止奶牛的死亡。“有些事并不像它看上去那样。”老天使答道，“当我们在地下室过夜时，我从墙洞看到墙里面堆满了金块。因为主人被贪欲所迷惑，不愿意分享他的财富，所以我把墙洞填上了。昨天晚上，死亡之神来召唤农夫的妻子，我让奶牛代替了她。所以有些事并不像它看上去那样。”
      
【温馨提示】

      有些时候事情的表面并不是它实际应该的样子。如果你有信念，你只需要坚信付出总会得到回报。你可能不会发现，直到后来……",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "对生活少些抱怨",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个学生请教老师，抱怨班里有某人特讨厌，总喜欢跟他比，影响了他的学习。
      老师问这学生：“你喜欢吃苹果吗？”
      学生愕然，但还是回答：“不喜欢，但喜欢吃雪梨。”
      “你不喜欢吃苹果？”
      “对”
      “那有没有人喜欢吃苹果”
      “当然有！”
      “那你不喜欢吃苹果是苹果的错吗？”
      笑笑，“当然不是！”
      “那你不喜欢他是他的错吗？”
      ……
      “你喜欢吃雪梨？”
      “对！”
      “如果你的好友来了，你会请他吃吗？”
      “会啊！”
      “你怎么知道他爱吃呢？”
      “问呗！”
      “那还好，但很多人就不是这样，觉得自己喜欢就以为他人也喜欢。”
      
【温馨提示】

      自己喜欢雪梨，以为鱼也会喜欢，所以当鱼饵放在钓钩上去钓鱼，鱼儿总是不上钩，于是就埋怨，这鱼儿怎么回事？古人说，子非鱼，安知鱼乎？",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "蝴蝶效应",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一只蝴蝶在巴西煽动翅膀，有可能在美国的得克萨斯洲引起一场龙卷风。
      一个微不足道的动作，或许会改变人的一生，这绝不是夸大其辞，可以作为佐证的事例随手便能拈来。美国福特公司名扬天下，不仅使美国汽车产业在世界占居熬头，而且改变了整个美国的国民经济状况，谁又能想到该奇迹的创造者福特当初进入公司的“敲门砖”竟是“捡废纸”这个简单的动作？
      那时候福特刚从大学毕业，他到一家汽车公司应聘，一同应聘的几个人学历都比他高，在其他人面试时，福特感到没有希望了。当他敲门走进董事长办公室时，发现门口地上有一张纸，很自然地弯腰把他捡了起来，看了看，原来是一张废纸，就顺手把它扔进了垃圾篓。董事长对这一切都看在眼里。福特刚说了一句话：“我是来应聘的福特”。董事长就发出了邀请：“很好，很好，福特先生，你已经被我们录用了。”这个让福特感到惊异的决定，实际上源于他那个不经意的动作。从此以后，福特开始了他的辉煌之路，直到把公司改名，让福特汽车闻名全世界。
      平安保险公司的一个业务员也有与福特相似的惊喜。他多次拜访一家公司的总经理，而最终能够签单的原因，仅仅是他在去总经理办公室的路上，随手捡起了地上的一张废纸并扔进了了垃圾桶。总经理对他说：“我（透过窗户玻璃）观察了一个上午，看看哪个员工会把废纸捡起来，没有想到是你。”而在这次见面总经理之前，他还被“晾”了3个多小时，并且有多家同行在竞争这个大客户。
      
【温馨提示】

      福特和业务员的收获看似偶然，实则必然，他们下意识的动作出自一种习惯，而习惯的养成来源于他们的积极态度，这正如著名心理学家、哲学家威廉?詹姆士所说：“播下一个行动，你将收获一种习惯；播下一种习惯，你将收获一种性格；播下一种性格，你将收获一种命运。”事实上，被科学家用来形象说明混沌理论的“蝴蝶效应”，也存在于我们的人生历程中：一次大胆的尝试，一个灿烂的微笑，一个习惯性的动作，一种积极的态度和真诚的服务，都可以出发生命中意想不到的起点，它能带来的远远不止于一点点喜悦和表面上的报酬。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "享受阳光",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个阳光灿烂的早晨，垂垂暮年的富翁坐在他的豪宅门口，看着门前来来往往的行人。富翁看到几个年轻人说说笑笑地走近，他们的脸上还留着稚气，质朴和衣着下透出无法掩饰的青春神采。富翁想，如果我能回到他们那样的年龄，即使只给我一年，我也愿意献出全部的财富。
      年轻人也看到了富翁，他们在豪宅前漫步走过，禁不住连连感叹。豪宅的大厅金碧辉煌，富翁的钻戒在太阳下闪烁着迷人的光彩。年轻人心想，要是能拥有富翁哪怕十分之一的财富，为此付出任何代价都在所不惜。
      年轻人走过之后，富翁感到很失落，他为岁月的无情而绝望。年轻人看到富翁拥有的财富之后，心里很嫉妒，他们觉得上帝很不公
      平。阳光灿烂的早晨，富翁和年轻人的心情都很灰暗。
      一个乞丐躺在豪宅墙外的马路边，因为阳光很好，他把旧衣裳一件件拿出来，晒在路边的树枝上。他眯起双眼，在灿烂的晨光中开始打盹。他没看见富翁，也没看见年轻人。他只看见了遍地阳光。
      
【温馨提示】

      在这个世界上，有的人总会发现自己比别人拥有得更多，有的人总是觉得自己比别从拥有得更少。所以，有的人总是快乐的，有的人总是在怨天尤人。珍惜已经拥有的一切吧，像乞丐珍惜阳光那样——享受阳光。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "快乐来自于心",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1914年夏天的那不勒斯城充满了从未有过的快乐，著名的滑稽大师马可尼用他精彩的表演使城里的每个人几乎笑破了肚皮。然而就在这期间，心理医生让?肯特的诊所里却来了一位神情沮丧的病人。他说：“大夫，我心里忧伤极了，多年来，我不愿见任何人，吃饭也没胃口，每晚入睡都靠镇静药的帮助。我怀疑我患了自闭症或其他什么心理疾病，我希望您能给我一些指导。”让?肯特大夫听了来者的叙述，说：“自从马可尼来这儿演出，我的诊所已经几天没有病人光顾了。我想，他们肯定是被马可尼逗得忘了病痛。现在马可尼还没有走，我建议您去看看他的演出，也许他会使您快乐起来。”患者脸上掠过一丝尴尬。他望着让?肯特，无奈地说：“大夫，我就是
      马可尼。”
      马可尼是20世纪10年代奥地利极富盛名的喜剧表演大师，让?肯特则是意大利著名的心理医生。据说，这次会面，对两人的触动都很大，让?肯特关闭诊所去了法国，马可尼回到故乡后便渐渐淡出了舞台。
      1957年，一位医生带领一个法国康复旅行团去奥地利旅行，在参观维也纳郊外的一座私人城堡时，他们得到了主人的热情接待。他虽已94岁高龄，但精神矍铄风趣幽默。他说，人是最笨的动物，各位客人来到这里如果打算向他学习，那就错了，应该向他家里的其他成员──巴迪、赖斯和莫莉学习。“我的狗巴迪不管遭受如何惨痛的欺凌和虐待，都会很快把痛苦抛在脑后，热烈地享受眼前的生命，细嚼能找到的每一根骨头；我的猫赖斯从不为任何事发愁，它如果感到焦虑不安，即使是最轻微的情绪紧张，都会去睡一觉，让不愉快的感觉尽快消失；我的鸟儿莫莉最懂得忙里偷闲，即使树丛里有吃不完的东西，它也会经常停下来，站在枝头唱一会儿。各位朋友，它们会让你们不虚此行！不过，我要警告给你们带队的老家伙，不要再劝他的病人去看马可尼的演出了。”他的话让在场的人都笑了起来．这座私人城堡的主人就是曾经名噪一时的喜剧大师马可尼，而那个带队的“老家伙”，就是著名的心理专家让?肯特医生。
      1963年，马可尼去世，年已九旬的让?肯特写了一篇文章──《怀念我的朋友马可尼》。在文章里。他回顾了他们50年的友谊，并且说，他之所以在心理学和大众医学研究方面能取得一点成就，完全得益于马可尼1914年的那次造访和1957年的那个忠告。
      
【温馨提示】

      如果一个人是快乐的，他不看喜剧表演也同样是快乐的；如果他不快乐，即使自己是喜剧大师也无济于事。快乐来自于心，就如脚上的鞋，有什么感觉只有你自己最清楚。如果想追求金钱、名誉和地位，你可以像别人一样去做；如果是追求快乐，你只要按自己的方式做就行了。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "面对金钱的考验",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      诺贝尔经济学奖获得者，美国心理学家丹尼尔?卡伊曼，第一次把心理学研究和经济学研究结合在一起，对人的行为，尤其是对不确定条件下的判断和决策行为提出了崭新的解释。他发现，风险决策后的输赢结果对人而言是不对等的，减少100元带给人的痛苦，远远大于增加100元带给人的愉快。他的基本结论是：人们最在乎的是他们已经得到的东西。
      我们是不是这样：
      1、收别人的钱时可以少收一角，给别人付钱时却不愿多付一分；
      2、有钱的人很容易成为守财奴，没钱的人却并不幻想成为富翁；
      3、攒了10天的100元钱丢了，只痛苦一天。攒了10年的100元钱丢了，要痛苦100天；
      4、欠别人的钱一天，可能一天都心有不安。如果欠了一年又一年，则可能永远心安理得；
      5、一个穷人可能会一辈子过着贫穷快乐的生活，而一个一夜之间变成穷人的富翁则可能一天都活不下去；
      6、没到手的钱很多人都不愿意伸手，到手的钱很多人都舍不得撒手；
      7、有人为了讨回自己的一元钱，甚至愿意花100元钱去打官司，但没有人愿意花100元钱的成本，去赚别人的10l元钱；
      8、有时，我们宁肯少挣100元钱，也不愿与人吵吵嚷嚷。但有时，我们为了少付10元钱，却不惜与人斤斤计较；
      9、煮熟的鸭子飞了，有人可能要发疯。活鸭子从眼前飞过，人们却可能无动于衷；
      10、在这个世界上，会省钱的人，总是比会赚钱的人多；
      11、如果没有那一笔钱，你可能不会觉得命运对你不公，如果失去那一笔钱，你就会觉得命运总是与你过不去；
      12、你可能为此惊奇过：如果你有一张百元大钞，10天后，你发现仍原封不动地装在身上；如果你有100元零钞，三天后，你发现竟在不知不觉中花光了；
      13、有钱的人想的是拿一分钱赚一分钱，没钱的人只是想怎样将一分钱掰成两半儿花；
      14、什么都没有的人看起来却像什么都有，什么都有的人看起来有多少都不够；
      15、在消费环节，富人总是赊账，比如签单，而穷人总是付现钱；
      16、在适当的时候，装成一个穷人，可能会对钱看得淡一些；同样，在适当的时候，装成一个富人，则可能对钱看得清一点；
      17、到了手的才是钱．
      18、在不同的钱面前，人与人不一样；在不同的人面前，钱与钱不一样；
      ......
      
【温馨提示】

      金钱的价值本来是人赋予的，但当它拥有了流通和交换的价值功能时，就开始无情地左右人的思想和行为。看一看我们的生活，有多少时候，我们的快乐或痛苦、宁静和烦恼是由我们自己做主？是金钱在每时每刻让我们流泪或欢笑。这到底是金钱的可恶，还是人自己的可怜？",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "自信+自足＝快乐？",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一位生活在费城的年轻人，整天唉声叹气，愁眉不展，逢人便吐苦水：“我实在是太不幸啦！父母没有给我留下遗产，我没有别墅，没有小汽车，甚至连到海边度一次假的钱都没有。”
      一位老者对年轻人说：“我有办法让你很有钱，但必须用你所拥有的东西来交换。你愿意吗？”
      年轻人高兴地说：“我有什么东西值钱吗？如果有，你要什么我都愿意，只要你能让我成为富翁。”
      “我出50万买你的一只手，你愿意吗？”
      “啊？一只手？我舍不得手。我不愿意！”青年人毫不犹豫地拒绝了。
      “那么，我用100万买你的一条腿，可以吗？”年轻人又坚决地摇了摇头。
      “一只眼睛呢？200万？”年轻人恐怖地直摇头。
      老者笑了：“你看，你现在至少已经拥有了350万，只是暂时还不想要这笔钱。年轻人，一个有手、有脚、有眼睛的人还怕没有钱吗？实际上，凭你所有的一切，你已经就是一位亿万富翁。一位亿万富翁还有什么可抱怨的呢？”
      听完老人的话，年轻人怔了片刻，羞愧地走了。
      
【温馨提示】

      在这个世界上我们什么都有了，如果我们不快乐，也许因为我们还缺少两样东西：一个是自足，一个是自信。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "快乐人生四句话",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位年轻人去后拜访一位智者。
      年轻人问：“我怎样才能变成一个自己愉快，同时也能给别人带来快乐的人呢？”
      智者笑着说：“孩子，在你这个年龄有这样的愿望，实属难得。我送给你四句话吧。”年轻人细心聆听智者的教诲。
      “第一句话：把自己当成别人。”
      年轻人说：“在我痛苦忧伤的时候，把自己当成别人，这样痛苦自然就减轻了；当我欣喜得意之时，把自己当成别人，那样，狂喜也会变得平和一些。是不是这样？”智者点点头。
      “第二句话：把别人当成自己。”
      年轻人沉思了一会儿，说：“这样就可以真正同情别人的不幸，理解别人的需要，并给予他人适当的帮助。”智者笑望着年轻人。
      “第三句话：把别人当成别人。”
      年轻人默默地思索着，然后抬头看着智者：“这句话是不是说，要充分尊重每个人的独立性，在任何情形下都不可侵犯他人的核心领地。”
      “很好，就是这样！”
      “第四句话是：把自己当成自己。”
      见年轻人似懂非懂，智者温和地说：“这句话理解起来也许太难，留着你以后慢慢品味吧！”
      年轻人沉吟很久，说：“我想，它至少包含有这样的意思，那就是：我们必须为自己负责。”智者微笑，未置可否。年轻人又问：“请问，这四句话怎样才能统一起来呢？”
      智者说：“很简单，用一生的时间和经历。”
      
【温馨提示】

      把自己当成别人是豁达，把别人当成自己是宽容，把别人当成别人是睿智，把自己当成自己是彻悟。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "68岁考耶鲁",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      阿克塞波青年时代喜欢学习语言，学习历史，喜欢阅读文学作品。当他从欧洲来到美国定居的时候，白天在磨坊干活，晚上就读书。但没过多久，他就结婚了，此后，他的精力全都用在应付农场的日常工作和家庭的各种开销上。多年过去了，他再也找不到时间学习。
      63岁那年，他决定退休。孩子们请他和他们同住，但阿克塞波拒绝了。“不，”他回答说，“你们搬到我的农场来吧。农场归你们管理，我上山去住，我在山上能望见你们。”
      他在山上修建了一间小屋，自己做饭，自己料理生活，闲暇时去公立图书馆借许多书回来阅读。他觉得自己从来没有生活得这么自在。他一反过去的习惯，早晨常常在床上躺到七八点钟，吃罢饭，往往还“忘记”打扫房间或清洗碗碟。后来，他甚至开始在夜间外出散步，他发现了黑夜的奥秘，他看到了月光下广阔的原野，听到了风中摇曳着草和树的声音。有时他会在一座小山头停下来，迎着风张开双臂，站在那里欣赏脚下沉睡的土地。
      阿克塞波从图书馆借来的书中有一本小说，这本小说让他感触很深。小说的主人公是一名耶鲁大学的青年学生，小说主要叙述他怎样在学业和体育方面取得的成就，还有一些章节描述了他丰富多彩的校园生活。一天凌晨，他读完了这本小说的最后一页，这时，68岁的他突然做出了一个决定：上大学，上耶鲁大学。他一辈子爱学习，现在他有的是时间，为什么不上大学？为了参加入学考试，他每天读书10个小时。他读了许多书，有几门学科他已有相当把握。于是他购置了几件像样的衣服，买了一张去康涅狄格州纽海芬的火车票，直奔耶鲁大学。
      他的考试成绩合格了，于是顺利被耶鲁大学录取。入学还不到两个星期，阿克塞波就发现，同学们对他似乎格外新鲜，不仅仅因为他年龄大，还因为他来上学的目的与众不同。别人选修的科目，都是为了有利于以后找工作、挣钱，而他和大家都不一样，他对有助于挣钱的科目不感兴趣。他是为快乐而学习，他学习的目的是要了解人类的过去和未来，了解世界的奥秘，弄清楚生活的目的，使自己的余生过得更有价值。教授对学生们说，阿克塞波才真正是在学习。几年后，阿克塞波完成了学业，并获得了学位，而且健康充实地活到95岁。
      
【温馨提示】

      学习永远不要觉得时间晚了，因为你不学习是过一天，学习同样也是过一天。过好你迎来的每一天，实施你的每一个决定，感受你正在感受的一切，收获你种下的所有果实──这就是生活！",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "魔鬼的金币",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在一间很破的屋子里，有一个穷人鲁弗斯，他穷得连床也没有，只好躺在一条长凳上。鲁弗斯自言自语地说：“我真想发财呀，如果我发了财，我就做一个慷慨的好人……”这时候，鲁弗斯身旁出现了一个魔鬼。魔鬼说：“我能让你发财，我会给你一个有魔力的钱。”
      魔鬼又说：“这钱袋里永远有一块金币，永远都拿不完。但是你要注意，在你觉得够了时，就要把钱袋扔掉，这时才可以开始花钱。”魔鬼说完话就不见了，鲁弗斯发现，身边真的有了一个钱袋，里面装着一块金币。鲁弗斯把那块金币拿出来，里面又有了一块。鲁弗斯不断地往外拿金币，一直拿了整整一个晚上，他已经有了一大堆金币。鲁弗斯想：这些钱已经够我用一辈子了。
      到了第二天．鲁弗斯很饿，很想去买面包吃。但是在他花钱以前，必须扔掉那个钱袋，于是便拎着钱袋向河边走去。可是来到河边他又舍不得扔，于是又回来了。鲁弗斯又开始从钱袋里往外拿钱。每次当他想把钱袋扔掉之前，总觉得钱还不够多。
      日子一天天过去了。鲁弗斯完全可以去买吃的、买房子、买最豪华的车子，可是他对自己说：“还是等钱再多一些吧！”鲁弗斯不吃不喝地工作着，金币已经快堆满屋子了。他变得又瘦又弱，脸色像蜡一样黄。鲁弗斯虚弱地说：“我不能把钱袋扔掉，我要源源不断的金币……。”鲁弗斯看起来已经有气无力了，但他还是颤抖着手往外掏金币。最后终于死在他的长凳上。
      
【温馨提示】

      知足是最难得的品质，永无止境的贪欲会毁掉我们的一切希望。贪欲就是魔鬼，欲望永远都有，而我们享受快乐的机会不是永远有，我们可以享受快乐的时光不是永远有。扔掉钱袋吧，你已经足够了……",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "你比谁都幸福",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      假如将全世界的人口压缩成一个100人的村庄，那么这个村庄将有：57名亚洲人，21名欧洲人，14名美洲人和大洋洲人，8名非洲人；52名女人和48名男人，30名白人和70名非基督教徒，89名异性恋和11名同性恋；6人拥有全村财富的89%，而这6人均来自美国；80人住房条件不好；70人为文盲；50人营养不良；1人正在死亡；1人正在出生；1人拥有电脑；1人（对，只有一人）拥有大学文凭。如果我们以这种方式认识世界，我们就可以理解下列信息：
      1、如果你今天早晨起床时身体健康，没有疾病，那么你比其他几千万人都幸运，他们甚至看不到下周的太阳；
      2、如果你从未尝试过战争的危险、牢狱的孤独、酷刑的折磨和饥饿的煎熬，那么你的处境比其他5亿人更好；
      3、如果你能随便进出教堂或寺庙而没有任何被恐吓、强暴和杀害的危险，那么你比其他30亿人更有运气；
      4、如果你的冰箱里有食物可吃，身上有衣可穿，有房可住，有床可睡，那么你比世界上75％的人更富有；
      5、如果你在银行有存款，钱包里有现钞，口袋里有零钱，那么你属于世界上8%最幸运的人；
      6、如果你父母双全没有离异，那你就是很稀有的地球人；
      7、如果你读了以上文字，说明你就不属于20亿文盲中的一员，他们每天都在为不识字而痛苦......
      
【温馨提示】

      善待别人、善待自己、享受阳光，享受生活就像这个世界是天堂。你若能够这样做，一定比生活在真正的天堂还幸福。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "花儿努力地开",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个人突然想学画，可是又犹豫不决，就去问他的老师：“再过4年，我就44岁了，能行吗？”老师对他说：“怎么不行呢？你不学画，再过4年也是44岁啊！”他想了想，瞬间开悟了，第二天就去艺术学校报了名。
      一个朋友，几年前跟人合伙做生意。不幸货船突遇风浪，他们的财产和梦想也随之坠人海底。经不起这个打击，他从此变得萎靡不振，神思恍惚。当他看到合伙人遭遇变故后依然如从前那样无忧无虑时，就去问他原因。那人对他说：“你咒骂，你伤心，日子一天天过去。你快活，你欢乐，日子也一天天过去。你选择哪一种呢？”
      
【温馨提示】

      人就是这样，当你以一种豁达、乐观向上的心态去构筑未来时，眼前就会呈现一片光明；反之，当你将思维困于忧伤的樊笼里，未来就变得暗淡无光了。长此下去，不仅最起码的信念和勇气会泯灭，身边那些最近最真的欢乐也将失去。对每一个人来说，那些如空气一样充塞在身边的欢乐才是最重要的，它构成了我们生命之链上最真实可靠的一环。如果你不用心抓住，它们就会一节一节地松落，欢笑怎么能向下延续呢？有一首诗写道：“你知道，你爱惜，花儿努力地开；你不知，你厌恶，花儿努力地开。”花儿总是在努力地开，美好的日子也一天天地自然流逝，你是欣喜地度过每一天，还是痛苦地挨过每一日？面对命运难道我们还不如一朵花吗？",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "爱创造奇迹",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      老丹尼斯是一名木匠。1945年的一天，他正在赶着做一批板条箱，那是教堂用来装衣服并运到中国去救助孤儿的。干完活回家的路上，丹尼斯伸手到衬衫口袋里摸他的眼镜，突然发现眼镜不见了。他在脑子里把这一天做过的事情重新回顾了一遍，然后他意识到发生了什么：在他不注意的时候，眼镜从衬衫口袋里滑落出去，掉进了其中一只正在打钉子的板条箱里。他崭新的眼镜就这样漂洋过海去了中国。当时美国正值大萧条时期，丹尼斯要养活6个孩子，生活非常艰难，而那副眼镜，是他刚花了20美元买来的。他为又要重新买一副眼镜的念头烦恼不堪。“这不公平，”在沮丧的回家途中，他嘀咕道，“上帝啊，我一向对你忠诚，把我的时间和金钱都奉献给你，可是现在，你看……”
      半年后，抗日战争胜利，中国那所孤儿院的院长——一个美国传教士回美国休假。在一个星期天，他来到了丹尼斯所在的这所芝加哥小教堂。他一开始就热忱地感谢了那些援助过中国孤儿的人们，“但最重要的，我必须感谢去年你们送给我的那副眼镜。”他说，“大家知道，日本人扫荡了孤儿院。毁坏了所有东西，包括我的眼镜，我当时已经绝望了，就算我有钱，也没有办法重新配一副眼镜。由于眼睛看不清楚，我开始头疼，我每天做的第一件事就是向上帝祈祷：万能的主啊，赐给我一副眼镜吧！就在这个时候，你们的箱子运到了。当我的同事打开箱盖，他们发现真的有一副眼镜躺在那些衣服上。”院长停顿了许久，好让自己的话音降低一些。然后，带着众人期盼的悬念，他继续说道：“各位朋友．你们也许不相信，当我戴上那副眼镜，我发现它就像是为我度身定做的一样！我的世界顿时清晰起来，头也不疼了。我要感谢你们，是你们为我做了这一切！”人们听着，纷纷为这副奇迹般的眼镜而欢欣，但是他们同时也在想，这位院长肯定是搞错了，我们可没有送过眼镜啊，在当初的援助物资目录上，根本没有眼镜这一项。只有一个人清楚这是怎么回事，他静静地站在后排，眼泪流到了脸上。在所有的人当中，只有这个普通的木匠知道，上帝是以怎样一种不同寻常的方式创造了奇迹。
      
【温馨提示】

      奇迹都是通过人类有爱的手创造的，通过人类有爱心传播的。如果我们自己不愿去创造奇迹，上帝只怕也无能为力；如果我们拥有坚定的信念，即使上帝缺席的时候，奇迹也会发生。我们坚信上帝，我们也要让上帝坚信我们。",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "别具一格的撑杆跳",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1904年，奥运会在美国圣路易斯堡举行。比赛正常进行着，但在撑杆跳高比赛时，却发生了意外。轮到日本运动员佐间代富士开始试跳，他没有像其他人那样飞奔冲向跳栏，而是从容不迫地慢慢走进沙坑，把手中的撑竿用力插进沙土里，固定好以后，就顺着竿子往上爬，一直爬到最高处，然后轻巧地翻过横竿。所有在场的人都看得目瞪口呆──怎么会有这样的撑竿比赛方法？全场嘘声一片。裁判犯难了，因为按照比赛规则，日本选手并没有犯规，只要利用撑竿跨过规定高度的横栏就可以了。然而经过裁判组的反复讨论，佐间代富士的成绩还是被取消了。佐间代富士据理力争，认为自己可以这样做。于是裁判组临时补充了一项撑竿比赛规则，那就是，要求运动员必须要有一段助跑过程。日本选手在第二次试跳时有了助跑动作。但跑到沙坑时他却停住，然后抓住撑竿，顺着竿子爬到最高的位置后再翻身滑下来。这一次他又取得了好成绩，同时再一次引起全场的骚动。裁判不得不承认，他又一次钻了规则的漏洞，按照修订的规则，佐间代富士仍然没有犯规。于是，裁判组再次召开紧急会议，确定撑竿比赛不仅要有助跑，并且不能交替使用双手的动作。这项规则确定下来之后，一直延用到现在。
      
【温馨提示】

      伟人制造规则，普通人执行规则──这似乎成了人类生存的惯例。但惯例并不是铁律，我们为什么不能挑战？规则都由人制定，这就注定它不是神圣不可侵犯的──我们为何不敢挑战？",
            group3));

            group3.Items.Add(new SampleDataItem("Group-4-Item-3",
            "向小布什总统推销一把斧子",
            "Item Subtitle: 3",
            "Assets/title/3.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      美国一位名叫乔治?赫伯特的推销员，成功地将一把斧头推销给了总统小布什。布鲁金斯学会得知这一消息，把刻有“最伟大推销员”的一只金靴子奖给了他。这是自1975年该学会一名学员成功地把一台微型录音机卖给尼克松以来，又一名学员成功登上如此高的推销门槛。
      布鲁金斯学会创建于1927年，以培养世界上最杰出的推销员著称于世。它有一个传统，在每期学员毕业时，设计一道最能体现推销员能力的实习题，让学生去完成。克林顿当政期间，他们出了这么一个题目：请把一条三角裤推销给现任总统。八年间，有无数个学员为此绞尽脑汁，可是，最后都无功而返。克林顿卸任后，布鲁金斯学会把题目换成：请将一把斧子推销给小布什总统。
      鉴于前八年的失败与教训，许多学员都知难而退。个别学员甚至认为，这道毕业实习题人和克林顿当政期间一样毫无结果，因为现在的总统什么都不缺少。再说即使缺少什么，也用不着他亲自购买。再退一步说，即使他亲自购买，也不一定正赶上你去推销的时候。然而，乔治?赫伯特却做到了，并且没有花多少功夫。一位记者在采访他的时候，他是这样说的：“我认为，将一把斧子推销给小布什总统是完全可能的。因为他在得克萨斯州有一座农场，里面长着许多树。于是我给他写了一封信，我说，有一次，我有幸参观您的农场，发现里面长着许多矢菊树，有些已经死掉，木质已变得松软。我想，您一定需要一把小斧头，但是从您现在的体质来看，这种小斧头显然太轻，因此您仍然需要一把不甚锋利的老斧头。现在我这儿正好有一把这样的斧头，它是我祖父留经我的，很适合砍伐枯树。假若您有兴趣的话，请按这封信所留的信箱，给予回复……最后他就给我汇来了15美元。”
      乔治?赫伯特成功后，布鲁金斯学会在表彰他的时候这样说：金靴子奖已空置了26年，26年间，布鲁金斯学会培养了数以万计的推销员，造就了数以百计的百万富翁，这只金靴子之所以没有授予他们，是因为我们一直想寻找这么一个人──他不因有人说某一目标不能实现而放弃，不因某件事情难以办到而失去自信。
      
【温馨提示】

      乔治?赫伯特的故事在世界各大网站公布之后，一些读者纷纷搜索布鲁金斯学会，他们发现，在该学会的网页上贴着这么一句格言：不是因为有些事情难以做到，我们才失去自信；而是因为我们失去了自信，有些事情才显得难以做到。",
            group3));



            this.AllGroups.Add(group3);
            #endregion

            #region 第4章
            var group4 = new SampleDataGroup("Group-4",
            "第4章",
            "Group Subtitle: 4",
            "Assets/title/4.png",
            "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
    "“蜘蛛人”汉姆曾是恐高症患者",
    "Item Subtitle: 3",
    "Assets/title/4.png",
    "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
    @"
      1983年，伯森?汉姆徒手登上纽约帝国大厦，在创造了吉尼斯纪录的同时，也赢得了“蜘蛛人”的称号。美国恐高症康复协会得知这一消息，致电“蜘蛛人”汉姆，打算聘请他做康复协会的心理顾问，因为在美国，有数万人患有恐高症，他们被这种疾病困扰着，有的甚至不敢站在椅子上换一只灯泡。伯森?汉姆接到聘书，打电话给协会主席诺曼斯，让他查一查他们协会里的第1042号会员情况。这位会员的资料很快被调了出来，他的名字叫伯森?汉姆，就是“蜘蛛人”自己。原来，这位创造了吉尼斯纪录的高楼攀登者，本身就是一位恐高症患者。诺曼斯对此大为惊讶。一个站在一楼阳台上都心跳加快的人，竟然能徒手攀上400多米高的大楼，这确实是个令人费解的谜，他决定亲自去拜访一下伯森?汉姆。
      诺曼斯来到费城效外汉姆的住所。这儿正在举行一个庆祝会，十几名记者正围着一位老太太拍照采访。原来伯森?汉姆94岁的曾祖母听说汉姆创造了吉尼斯纪录，特意从一百公里外的葛拉斯堡罗徒步赶来，她想以这一行动，为汉姆的纪录添彩。谁知这一异想天开的想法，无意间竟创造了一个百岁老人徒步百里的世界纪录。
      《纽约时报》的一位记者问她：当你打算徒步而来的时候，你是否因年龄关系而动摇过？老太太精神矍铄，朗朗地笑着说：小伙子，打算一气跑一百公里也许需要勇气，但是走一步路是不需要勇气的，只要你走一步，接着再走一步。然后一步再一步，一百公里也就走完了。恐高症康复协会主席诺曼斯紧接着问伯森?汉姆：你的诀窍是什么？伯森?汉姆看着自己的曾祖母说：我和曾祖母一样，虽然我害怕400多米高的大厦，但我并不恐惧一步的高度。所以，我战胜的只是无数个“一步”而已。
      
【温馨提示】

      我们也许没有能力一次就取得一个大成功，但我们可以积累无数个小成功。一个小成功并不能改变什么，但无数的小成功加起来就可以让我们成为巨人。",
    group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "德国的韦博图山",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在中世纪的德国，那年是1141年，巴伐利亚公爵沃尔夫被困在了他的温斯堡城中，城堡之外是斯瓦比公爵弗雷德里克及其兄长康
      纳德国王的军队。围攻已历时数月，沃尔夫知道，他只能投降了。信使开始在两军之间频繁穿梭，投降的条款列出来了，条件被应允了，所有的安排都完备了，沃尔夫和他的军官们准备将自己交给死敌。但是温斯堡里的女人们还没有打算放弃一切。她们给康纳德国王送去
      口信，要求许诺保证温斯堡内所有女人和儿童的安全，并且允许她们离开时，带走她们双手能够带走的所有东西。她们的要求被准许了，接着，城堡的大门打开了。女人们走了出来──城堡外所有的人都对看到的一切大吃一惊：每个女人的腰部弯得低低的，但她们手里拿着的不是金子，也不是珠宝，而是紧紧抱着她们的丈夫。她们要救出她们的男人，不能让自己的男人受到这支获胜军队的报复。
      康纳德，这位仁慈的国王，据说被这一壮举感动得流下了眼泪。他立即向这些女人宣布，保证她们的丈夫有完全的安全与自由。接着，国王与巴伐利亚公爵签订了和平条约，条约中的款项比公爵事先预想的要友善很多。因为，智慧的康纳德国王知道，一个拥有爱心的群体是不会被征服的。
      从此以后，温斯堡更名为“韦博图山”。“韦博图”在德语中的意思是“女人的坚贞”。
      
【温馨提示】

      勇气源于真爱，智慧源于真爱。只有爱才可以激起最强大的责任感，强大的责任感可以让一个人正直无畏的面对一切。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "穆律罗的骄傲",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      穆律罗是17世纪西班牙最有名的画家和贵族。在他众多的奴仆中，有一名叫塞伯斯蒂的青年人对绘画有一种与生俱来的热爱。每当穆律罗给学生上课时，塞伯斯蒂就在一旁偷偷观摩。
      一天晚上，塞伯斯蒂一时兴起竟然在主人的画室里画起画来，以至于第二天早晨穆律罗和一群贵族朋友出现时，他都没有发现。穆律罗并没有惊动塞伯斯蒂，而是静静地望着他笔下优美的线条出神。塞伯斯蒂画完最后一笔，才发现身后的主人，他慌忙跪下，恳求主人饶恕。在那个等级森严的年代里，塞伯斯蒂是可以因此而被主人处死的。
      这件事马上成了贵族们津津乐道的话题，就在他们纷纷猜测穆律罗会以何种方式严惩他的奴仆时，却听到了一个令人震惊的消息：穆律罗不仅给了塞伯斯蒂自由，而且要收他为徒。这是当时的贵族们绝不允许的，他们开始疏远穆律罗，也不再去买他的画，人们都说穆律罗是个十足的傻瓜。穆律罗对此却不以为然，他只是一笑：那些傻瓜怎能明白，塞伯斯蒂将会是我穆律罗最大的骄傲？
      事实果如穆律罗所言，在今天意大利的艺术馆藏中，塞伯斯蒂的作品与他恩师穆律罗的名画被摆在同等重要的位置，而且都价值连城。人们只要提到塞伯斯蒂，一定要提到穆律罗的名字。
      意大利人是这样看待这件事的：他们是17世纪最杰出的两位画家，他们是师徒，都很伟大，那些说穆律罗是傻瓜而没有买他画的人，才是世上最大的傻瓜。
      
【温馨提示】

      改变一个人命运的，往往是他自身的才华，而不是出身，塞伯斯蒂证实了这一点；一个受后人尊敬的人，不仅仅因为他的传世作品，更是因为他的人格和胆识，穆律罗正是如此。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "把鲜花送给对手",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      这是一场激烈的世界职业拳王争霸赛。正在比赛的是美国两个职业拳手，年长的叫卡菲罗，35岁，年轻的叫巴雷拉，28岁。上半场两人打了六个回合，实力相当，难分胜负。在下半场第七个回合，巴雷拉接连击中老将卡菲罗的头部，打得他鼻青脸肿。短暂的休息时，巴雷拉真诚地向卡菲罗致歉。他先用自己的毛巾一点点擦去卡菲罗脸上的血迹。然后把矿泉水洒在他的头上。巴雷拉始终是一脸歉意，仿佛这一切都是自己的罪过。接下来两人继续交手。也许是年纪大了，也许是体力不支，卡菲罗一次又一次地被巴雷拉击倒在地。按规则，对手被打倒后，裁判连喊三声，如果三声之后仍然起不来，就算输了。每次卡菲罗都顽强地挣扎着起身，每次都不等裁判将“三”叫出口，巴雷拉就上前把卡菲罗拉起来。卡菲罗被扶起后，他们微笑着击掌，然后继续交战。裁判和观众都感到吃惊，这样的举动在拳击场上极为少见。最终，卡菲罗以108：110的成绩负于巴雷拉。观众潮水般涌向巴雷拉，向他献花、致敬、赠送礼物。巴雷拉拨开人群，径直走向被冷落一旁的老将卡菲罗，将最大的一束鲜花送进他的怀抱。两人紧紧地拥在一起，相互亲吻对方被击伤的部位，俨然是一对亲兄弟。卡菲罗真诚地向巴雷拉祝贺，一脸由衷的笑容。他握住巴雷拉的手高高举过头顶，向全场的观众致敬。
      卡菲罗虽然败了，但败得很有风度；巴雷拉赢了，却赢得十分大气。
      
【温馨提示】

      在自己失败的时候，能够坦然为成功者庆贺，表现出的是一种难得的宽容和自信；在自己胜利的时候，热情地送给失败者旧鲜花，这是一种人格境界上的更大成功──无论哪一种，都需要真诚的勇气。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "马歇尔拒当元帅",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      乔治?马歇尔是美国的一代名将，在第二次世界大战中，他作为美国陆军参谋对建立国际反法西斯统一战线做出了重要贡献。鉴于其卓越功勋，1943年，美国国会同意授予马歇尔美国历史上从未有过的最高军衔──陆军元帅。但马歇尔坚决反对，他的公开理由是如果称他“Fieldmarshal”很别扭。其实真正的原因是这将使他的军衔高于当时已病倒的陆军四星上将潘兴。在马歇尔心目中，潘兴才是美国当代最伟大的军人，自己又多受潘兴提拔和力荐之恩，马歇尔不愿使他崇敬的老将军的地位和感情受到伤害。
      马歇尔拒绝当元帅后，为了表示对他的敬意，美军从此不再设元帅军衔。1944年底，马歇尔晋升五星上将──美军的最高军衔。
      
【温馨提示】

      面对别人的夸赞与肯定时，能保持清醒与自省的人，往往能得到别人更多的肯定和称赞，因为成就他的不是别人的称赞，而是自己的真才实学和人格魅力。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "温泽市政府大厅的支柱",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      他是英国一位年轻的建筑设计师，很幸运地被邀请参加了温泽市政府大厅的设计。他运用工程力学的知识，根据自己的经验，很巧妙地设计了只用一根柱子支撑大厅天顶的方案。一年后，市政府请权威人士进行验收时，对他设计的一根支柱提出了异议，他们认为，用一根柱子支撑天花板太危险了，要求他再多加几根柱子。年轻的设计师十分自信，他说，只要用一根柱子便足以保证大厅的稳固。他详细地通过计算和列举相关实例加以说明，拒绝了工程验收专家们的建议。他的固执惹恼了市政官员，年轻的设计师险些因此被送上法庭。在万不得已的情况下，他只好在大厅四周增加了4根柱子。不过，这四根柱子全部都没有接触天花板，其间相隔了无法察觉的两毫米。
      时光如梭，岁月更迭，一晃就是300年。300年的时间里，市政府官员换了一批又一批，市府大厅坚固如初。消息传出，世界各国的建筑师和游客慕名前来，观赏这几根神奇的柱子，并把这个市政大厅称作“嘲笑无知的建筑”。最为人们称奇的，是这位建筑师当年刻在中央圆柱顶端的一行字：自信和真理只需要一根支柱。
      这位年轻的设计师就是克里斯托?莱伊恩，一个很陌生和名字。今天，能够找到有关他的资料实在微乎其微了，但在仅存的一点资料中，记录了他当时说过的一句话：“我很相信。至少100年后，当你们面对这根柱子时，只能哑口无言，甚至张口结舌。我要说明的是，你们看到的不是什么奇迹，而是我对自信的一点坚持。”
      
【温馨提示】

      坚持真理需要自信支柱。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "推开一扇门并不难",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      从前，有一位国王，决定出一道题考一考他的大臣，以便从中选拔出智慧勇敢的人担任国中要职。他把臣子们领到一扇奇大无比的门前说：“这是我们王国中最大的门，也是最重的门。请问，你们当中谁能把它打开？”大臣们都知道，这扇门过去从没打开过，所以。他们认为这门肯定是打不开的。于是，一些大臣望着门不住地摇头。另一些人则装腔作势地走上前去看一阵，但并不动手，因为他们不想当众出丑，还有人甚至猜想，国王或许另有用意，所以，静观其变才是最稳妥的态度。这时，有一位年轻的大臣向大门走了过去，只见他双手猛力向大门推去，门被豁然打开了。原来，这扇门本来就是虚掩着的，没有锁也没有插栓，任何人都能轻易地推开它。这个大臣最终得到了国王的奖赏，并获得了重要的职位。
      
【温馨提示】

      歌德曾说：“你若失去财产，你只失去了一点儿；你若失去了荣誉，你就失去了许多；你若失去了勇敢，你就失去了全部。”许多人认为，人生的成功可能需要许多条件，其实只有勇敢就足够了。成功的人们并不比我们更有知识、更加聪明，他们和我们惟一的不同是：比我们更有冒险的勇气。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "泥人过河",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一天，上帝宣旨说，如果哪个泥人能够走过指定的那条河流，他就会赐给这个泥人一颗永不消失的金子心，赐给他天堂的美景。
      这道旨意下达后，泥人们久久都没有回应。不知道过了多久，终于有一个小泥人站了出来。
      “泥人怎么可能过河呢？你不要做梦了。”
      “你知道，肉体一点点失去时是什么感觉？”
      “你将会成为鱼虾的美味，连一根头发都不会留下。”
      然而，这个小泥人决意要过河。他不想一辈子只做个小泥人，他想拥有自己的天堂，想拥有一颗永不消失的金子心。但是他知道，要到天堂，得先过地狱。而他的地狱，就是他将要经历的这条河。
      小泥人来到河边，犹豫了片刻，他的双脚终于踏进水中。一种撕心裂肺的痛楚顿时覆盖了他，他感到自己的脚在飞快地溶化，灵魂正一分一秒地远离自己的身体。
      “快回去吧，不然你会毁灭的！”河水咆哮着说。
      小泥人没有回答，只是沉默着忍受巨痛往前挪动，一步，又一步。这一刻，他忽然明白，他的选择使他连后悔的机会都没有了。如果倒退上岸，他就是一个残缺的泥人；如果在水中迟疑，只能加快自己的毁灭。而上帝给他的承诺，却遥不可及。小泥人孤独而倔强地走着。这条河真宽啊，仿佛耗尽一生也走不到尽头。他向对岸望去，看见了美丽的鲜花、碧绿的草地和快乐飞翔的小鸟。也许那就是天堂的生活，可是他付出了一切也似乎不能抵达。上帝没有赐给他出生在天堂成为花草的机会，也没有赐给他一双小鸟自己放弃了安稳的生活。
      小泥人继续向前挪动，一厘米，一厘米，又一厘米……鱼虾贪婪地咬着他的身体，松软的泥沙使他摇摇欲坠，有无数次，他都被波浪呛得几乎窒息。小泥人真想躺下来休息一会儿啊，可他知道，一旦躺下来，他就会永远站不起来了，连痛苦的机会都会失去。他只能忍受、忍受、再忍受。奇妙的是，每当小泥人觉得自己就要死去的时候，总有什么东西使他能够坚持到下一刻。不知道过了多久──简直就到了让小泥人绝望的时候，他突然发现，自己居然上岸了。他如释重负，欣喜若狂，正想往草坪上走，又怕自己身上的泥土玷污了天堂的洁净。他低下头，开始打量自己，却惊奇地发现，他的身体已经不再是泥土──他已经变成了一颗金灿灿的心！
      
【温馨提示】

      在生活中我们不就是在趟过这样一道又一道的河吗？勇敢地舍去才能得到更多！",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "惜小失大",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      现年55岁的岁的迈克?莱恩，曾是英国一名出色的皇家探险队员。之所以有很多人知道他的名字，是因为他的一次非凡壮举。
      1976年，他随英国探险队成功登上了一般说来珠穆朗玛峰。但在下山的路上，他们却遇上了狂风暴雪。风雪根本没有停止的迹象，而他们的食品已所剩不多。如果停下来扎营休息，他们可能在没有下山之前就被饿死；如果继续前行，因为大部分路标被积雪覆，他们必须要走许多弯路，每个队员身上所带的增氧设备及行李会压得他们喘不过气来，也许不等他们饿死，就会因疲劳而倒下。在整个探险队陷入迷茫的时候，迈克?莱恩率先丢弃所有的随身装备，只留下不多的食品──他决定轻装前行。这一举动几乎遭到所有队友的反对，他们认为，下山最快的速度也要10天时间，这就意味着10天之中不仅不能安营休息，还可能因缺氧而使体温下降冻坏身体。那样，他们的生命将会极其危险。迈克?莱恩坚定地告诉他们：“我们必须而且只能这样做。这样的天气10天甚至半个月都可能不会好转，再拖下去，路标也许会全部被掩埋，那时，我们就走投无路。现在我们把所有的重物丢掉，从此不再抱任何幻想，全部的意念就指向一个目标──走出暴风雪。徒手而行可以大大提高速度，只要我们有信心，就一定有生的希望！”队友们采纳了他的建议，他们一路互相鼓励，忍受疲劳和寒冷，结果用了8天时间到达安全地带。正像他们预料的那样，恶劣的天气一直持续了半个多月。
      后来，伦敦国家博物馆的工作人员找到迈克?莱恩，请求他赠送一件与当年探险队登上珠穆朗玛峰有关的纪念物品，结果他们收到了莱恩几个冻掉的脚趾。其中还附有他亲笔写的一句话：真正的勇士，是那些关键时刻敢于放弃的人。
      
【温馨提示】

      吝惜小的东西，就不得不失去大的东西；对眼前“拥有”的贪恋，终会导致将来“失去”的痛苦。迈克?莱恩正好相反，虽然他失去了几个脚趾，却保住了生命──因为他能够放下，因为他敢于放弃。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "要敢于忏悔和认错",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      故事发生在1954年的岁末。那时，杰克只有12岁。他是一个勤劳懂事的孩子，上学之余，还给附近的邻居送报纸，以此赚取他所需要的零用钱。在他送报的客户中，有一位慈祥善良的老夫人。现在杰克已经记不起她的姓名了，但她曾经给他上的一堂有价值的人生课，他依然记忆犹新．杰克从来都没忘记过这件事，他希望有一天能把它传授给别人，让他们也从中得到教益。
      在一个风和日丽的午后，杰克和一个小朋友躲在那位老夫人家的后院里，朝她的房顶上扔石头。他们饶有兴味地注视着石头像子弹一样飞出去，又像彗星一样从天而降，并发出很响的声音。他们觉得这样玩很开心、很有趣。杰克又拾起一枚石头，也许因为那块石头太滑了，当他掷出去的时候，一不小心，石头偏了方向，一下子飞到老夫人后廊的一面窗户上．当他们听到玻璃破碎的声音时，就像兔子一样从后院逃走了。那天晚上，杰克一夜都没睡着，一想到老夫人家的玻璃就很害怕，他担心会被她抓住。很多天过去了，一点动静都没有。他确信已经没事了，但内心的犯罪感却与日俱增。他每天给老夫人送报纸的时候，她仍然微笑着和他打招呼，而杰克却觉得很不自在。杰克决定把送报纸的钱攒下来，给老夫人修理窗户。三个星期后，他已经攒下7美元，他计算过，这些钱已经足够了。他写了一张便条，把钱和便条一起放在一个信封里。他向老夫人解释了事情的来龙去脉，并且说出了自己的歉意，希望能得到她的谅解。杰克一直等到天黑才小心翼翼地来到老夫人家，把信封投到她家门口的信箱里。他的灵魂感到一种赎罪后的解脱，重新觉得自己能够正视老夫人的眼睛了。第二天，他又去给她送报纸，这次杰克坦然地对她说了一声“您好，夫人！”她看起来很高兴，说了“谢谢”之后，就递给杰克一样东西。她说：“这是我给你的礼物。”原来是一袋饼干。吃了很多块饼干之后，杰克突然发现袋子里有一个信封。他小心将信封打开，发现里面装了7美元纸钞和一张彩色信笺。信笺上大大地写着一行字：“诚实的孩子，我为你感到骄傲。”
      
【温馨提示】

      做了错事是遗憾的，如果做了错事还加以掩盖，还挖空心思躲避谴责，那么这就是再大的遗憾了。人们愿意谅解一个做了错事的人，但决不原谅一个掩饰错误的人。因为做错事可能是无意的，但回避谴责一定是有意的。敢于忏悔和认错的人是永远值得尊敬的。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "不能流泪就微笑",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在美国艾奥瓦州的一座山丘上，有一间不含任何合成材料、完全用自然物质搭建而成的房子。里面的人需要依靠人工灌注的氧气生存，并只能以传真与外界联络。住在这间房子里的主人叫辛蒂。1985年，辛蒂还在医科大学念书，有一次，她到山上散步，带回一些蚜虫。她拿起杀虫剂为蚜虫去除化学污染，这时，她突然感觉到一阵痉挛，原以为那只是暂时性的症状，谁料到自己的后半生就从此变为一场噩梦。
      这种杀虫剂内所含的某种化学物质，使辛蒂的免疫系统遭到破坏，使她对香水、洗发水以及日常生活中接触的一切化学物质一律过敏，连空气也可能使她的支气管发炎。这种“多重化学物质过敏症”是一种奇怪的慢性病，到目前为止仍无药可医。患病的前几年，卒蒂一直流口水，尿液变成绿色，有毒的汗水刺激背部形成了一块块疤痕。她甚至不能睡在经过防火处理的床垫上，否则就会引发心悸和四肢抽搐──辛蒂所承受的痛苦是令人难以想像的。1989年，她的丈夫吉姆用钢和玻璃为她盖了一所无毒房间，一个足以逃避所有威胁的“世外桃源”。辛蒂所有吃的、喝的都得经过选择与处理，她平时只能喝蒸馏水，食物中不能含有任何化学成分。
      多年来，辛蒂没有见到过一棵花草，听不见一声悠扬的歌声，感觉不到阳光、流水和风的快慰。她躲在没有任何饰物的小屋里，饱尝孤独之苦。更可怕的是，无论怎样难受，她都不能哭泣，因为她的眼泪跟汗液一样也是有毒的物质。坚强的辛蒂并没有在痛苦中自暴自弃，她一直在为自己，同时更为所有化学污染物的牺牲者争取权益。
      辛蒂生病后的第二年就创立了“环境接触研究网”，以便为那些致力于此类病症研究的人士提供一个窗口。1994年辛蒂又与另一组织合作，创建了“化学物质伤害资讯网”，保证人们免受威胁。目前这一资讯网已有5000多名来自32个国家的会员，不仅发行了刊物，还得到美国上议院、欧盟及联合国的大力支持。在最初的一段时间里，辛蒂每天都沉浸在痛苦之中，想哭却不敢哭。随着时间的推移，她渐渐改变了生活的态度，她说：“在这寂静的世界里，我感到很充实。因为我不能流泪，所以我选择了微笑。”
      
【温馨提示】

      当灾难降临，人可以努力回避；如果回避不了，可以抗争；如果抗争不了，就得承受；要是承受不了，就哭泣流泪；如果连流泪也不行，可能就只有一种造反；绝望和放弃。生活并非是我们想象的那样已由上帝安排定局，如果你不喜欢，一切都可以改变！象辛蒂那样无法流泪时选择微笑！",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "南瓜和铁的较量",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      美国麻省Amherst学院进行了一个很有意思的实验。
      实验人员用很多铁圈将一个小南瓜整个箍住，以观察它逐渐长大时，能抗住多大由铁圈给予它的压力。当初实验员估计南瓜最多能够承受500磅的压力。在实验的第一个月，南瓜就承受了500磅的压力；实验到第二个月时，这个南瓜承受了1500磅的压力；当它承受到2000磅的压力时，研究人员开始对铁圈进行加固，以免南瓜将铁圈撑开。当研究结束时，整个南瓜承受了超过5000磅的压力，到这时候，瓜皮因为巨大的反作用力产生破裂。他们取下铁圈，费了很久的力气才打开南瓜。它已经无法食用，因为试图想突破重重铁圈的压迫．南瓜中间充满了坚韧牢固的层层纤维。为了吸收充分的养分，以便于提供向外膨胀的力量，南瓜的根系总长甚至超过了8万英尺，所有的根不屈地往各个方向伸展，儿乎穿透了整个花园的每一寸土壤。
      
【温馨提示】

      我们无法想象一个南瓜能承受如此大的压力。人在顺境中也无法想象自己到底能经受多大的挫折。假如南瓜能够承受如此庞大的压力，那么人也一定能够承受。生命的潜能永远大于我们对它的估价！",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "金子与大蒜",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      传说有一位商人，带着两袋大蒜，骑着骆驼，一路跋涉到了遥远的阿拉伯。那里的人们从没有见过大蒜，更想不到世界上还有味道这么好的东西，因此，他们用当地最热情的方式款待了这位聪明的商人，临别赠与他两袋金子作为酬谢。
      另有一位商人听说了这件事后，不禁为之动心，他想：大葱的味道不也很好么？于是他带着葱来到了那个地方。那里的人们同样没有见过大葱，甚至觉得大葱的味道比大蒜的味道还要好！他们更加盛情地款待了商人，并且一致认为，用金子远不能表达他们对这位远道而来的客人的感激之情，经过再三商讨，他们决定赠与这位朋友两袋大蒜！
      
【温馨提示】

      生活往往就是这样，你先抢一步，占尽先机，得到的是金子；而你步人后尘，东施效颦，得到的可能就是大蒜！",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "低头走完一生的人",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一对夫妇一直渴望有个孩子，但是，他们却等了10多年才生了个儿子，总算如愿以偿了。儿子是他们的掌上明珠，夫妇俩想尽办法教导儿子，生怕他吃一点点苦头，他们连走路的方式也清清楚楚地告知儿子：“我的好孩子，走路时记得要看着地上啊！如果你走在木板上要专心看着脚底下，因为木板最容易让人滑倒。”这是儿子开始学习走路时爸爸的叮咛。乖巧的儿子也相当遵从父亲的教导，只要走在木质地板上，他一定紧盯着脚下。
      有一天，他们一家人来到山间游玩，爸爸又教导儿子：“在山路上行走时，你还是要看着地上，每一步都要相当小心，不然你会从山顶摔到山谷中；而下山坡时，你一样要看着脚下，否则一个闪失，你就会扭伤脚踝的，知道吗？”儿子点了点头，说：“是的，爸爸！”
      有一天，儿子准备到海边旅行，妈妈连忙叮嘱他：“儿子啊！当你走在沙滩上时，千万要小心啊！双眼一定要紧盯着脚下，因为海浪随时都会出现，幸运点儿只会溅湿你的裤子，不幸运的话它会将你卷入海里。”
      不久，父母相继离开了儿子。可怜的儿子逐渐长大了，他从小就习惯听爸爸妈妈的引导与叮咛，如今他只能在过去的叮咛中继续生活；对于父母的话，他仍然相当遵从。
      儿子认真执行父母的叮嘱，在木板上、在田野间、在上山与下山时，他都用心地盯着脚下。即使来到沙滩上，听见美丽的浪潮声，他也不会抬头看看声音是从哪里来的。不管走到哪里，“听话”的儿子，总是低着头往前走。
      儿子从来没有跌倒过，也没有滑倒或碰伤过，几乎毫发无伤的他，就这么“低着头”走过完他的一生。不过，在他临死前，他仍然不知道，原来天空是蓝色的，天上不仅有美丽的云彩，还有耀眼的星星。此外，他也不知道自己所走过的生一个地方，风光是多么美丽。
      
【温馨提示】

      文中的儿子一生遵从父母的叮嘱，小心翼翼地走完了一生，虽然他在成长道路上的任何尝试都毫发无伤，但他错过了很多美丽的风光。在实践中，谨慎是好的，但我们应放开自己出入进去，不要瞻前顾后，这样我们才会有更多的收获。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "穆罕默德唤山",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一位著名的经济学教授，凡是被他教过的学生，鲜少有顺利拿到学分的。原因出在，教授平时不苟言笑，教学古板，分派作业既多且难，学生们不是选择逃学，就是打混摸鱼，也不愿多听老夫子讲一句。但这位教授可是国内首屈一指的经济学专家，叫得出名字的几位财经人才，都是他的得意门生。谁若是想在经济学这个领域内闯出一点儿名堂，首先得过了他这一关才行！
      一天，教授身边紧跟着一名学生，二人有说有笑，惊煞了旁人。后来，就有人问那名学生说：“干嘛对那种八股教授跟前跟后的巴结呀！你有一点儿骨气好不好！”那名学生回答：“你们听过穆罕默德唤山的故事吗？穆罕默德向群众宣称，他可以叫山移至他的面前来，等呼唤了三次之后，山仍然屹立不动，丝毫没有向他靠近半寸；然后，穆罕默德又说，山既然不过来，那我自己走过去好了！教授就好比是那座山，而我就好比是穆罕默德，既然教授不能顺从我想要的学习方式，只好我去适应教授的授课理念。反正，我的目的是学好经济学，是要入宝山取宝，宝山不过来，我当然是自己过去喽！”
      这名学生，果然出类拔粹，毕业后没几年，就成为金融界响叮当的人物，而他的同学，都还停留在原地“唤山”呢！
      
【温馨提示】

      随着外在环境的变异而调整适应能力，要比一厢情愿地抛出自我的吶喊等待回响，来得有智慧多了。有时，一种来自于适应后的融入，反而更能激发出生命的潜能。等到你具备了一定的条件与能力时，该适应你的，自然就会臣服了。小学与中学的教学方式是不同的，但学校毕竟是从事教育的专业机构，bb们有个适应过程，BBMM们适当放手让孩子适应吧！",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "苍蝇与蜜蜂",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      美国康奈尔大学的威克教授曾做过一个实验：把几只蜜蜂放进一个平放的瓶子中，瓶底向着有光的一方，瓶口敞开。但见蜜蜂们向着有光亮处不断飞动，不断撞在瓶壁上。最后当他们明白，自己永远都飞不出这个瓶底时，于是不愿再浪费力气,它们停在光亮的一面，奄奄一息。
      威克教授于是倒出蜜蜂，把瓶子按原样放好，再放入几只苍蝇。不到几分钟,所有的苍蝇都飞出去了。原因很简单，苍蝇们并不朝着一个固定的方向飞行，它们会多方尝试，向上、向下、向光、背光，一方通立刻改变方向,虽然免不了多次碰壁，但它们最终会飞向瓶颈,并顺着瓶口飞出。它们用自己的不懈努力改变了像蜜蜂那样的命运。
      威克教授于是总结出一个观点:横冲直撞总比坐以待毙要高明得多。
      
【温馨提示】

      成功并没有什么秘诀,就是在行动中尝试、改变、再变、再尝试……直到成功。有的人成功了,只因为他比我们犯的错误、遭受的失败更多。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "好孩子是夸出来的",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有这样一个故事：一位父亲在冬天砍掉一棵枯树，到了春天，他惊奇地发现树桩上又萌发了一圈新绿。于是父亲对孩子说：“当时我真的以为这棵树已经死了，树叶掉得一片不剩，光秃秃的枝丫也不断地往地上落。现在才知道，它看似枯死的躯干还蕴藏着活力。孩子，不要忘了这个教训，不要在冬天里砍树。对于处于逆境中的事物，决不要事先得出消极的结论。耐心等待，冬天会过去，春天会到来。”
      在冬天看见春天，是一种智慧，但这种智慧往往被我们忽略。人生所遭受的心灵砍伐总是屡见不鲜的，这从我们的孩童时期就开始了。一有过错，家长就会发怒：你一辈子都不会有出息；成绩不好，老师会指责：你蠢笨如驴，不会有什么作为……这些简单武断的否定，无形之中就扼杀了孩童与生俱来的天分，让本来很有潜力的孩子，过早就失去了活力，倒在自卑的阴影里。这与在冬天砍伐一棵树有着一样的道理。
      卡耐基小时候是一个公认的非常淘气的坏男孩。在他9岁的时候，父亲把继母娶进家门。当时他们是居住在弗吉尼亚州乡下的贫苦人家，而继母则来自较好的家庭。他父亲一边向她介绍卡耐基，一边说：“亲爱的，希望你注意这个全县最坏的男孩，他可让我头疼死了，说不定会在明天早晨以前就拿石头扔向你，或者做出别的什么坏事，总之让你防不胜防。”出乎卡耐基意料的是，继母微笑着走到他面前，托起他的头看着他，接着又看着丈夫说：“你错了，他不是全县最坏的男孩，而是最聪明、但还没有找到发泄热忱地方的男孩。”继母说得卡耐基心里热乎乎的，眼泪几乎滚落下来。就凭着她这一句话，他和继母开始建立友谊。也就是这一句话，成为激励他的一种动力，使他日后创造了成功的28项黄金法则，帮助千千万万的普通人走上成功和致富的光明大道。因为在她来之前没有一个人称赞过他聪明。
      
【温馨提示】

      最残酷的伤害是对一个人的自信心的伤害，最大的帮助是给人以能支撑起人生信念风帆的信任和赞美。不论你的孩子现在是多么的“差”，你都要多鼓励孩子，并善于鼓励孩子，充分树立起他们的自信，在人生的长河中做到信念永存，脚踏实地，你的孩子就一定会步入成功的殿堂。好孩子是夸出来的！",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "困境即是赐予",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一天，素有森林之王之称的狮子，来到了天神面前:“我很感谢你赐给我如此雄壮威武的体格,如此强大无比的力气，让我有足够的能力统治这整座森林。”天神听了,微笑地问:“但是这不是你今天来找我的目的吧！看起来你似乎为了某事而困扰呢！”狮子轻轻吼了一声，说：“天神真是了解我啊！我今天来的确是有事相求。因为尽管我的能力再好，但是每天鸡鸣的时候，我总是会被鸡鸣声给吓醒。神啊！祈求您，再赐给我一个力量，让我不再被鸡鸣声给吓醒吧！”天神笑道：“你去找大象吧，它会给你一个满意的答复的。”狮子兴匆匆地跑到湖边找大象，还没见到大象，就听到大象跺脚所发出的“砰砰”响声。狮子加速地跑向大象，却看到大象正气呼呼地直跺脚。狮子问大象：“'你干嘛发这么大的脾气？”大象拼命摇晃着大耳朵，吼着：“有只讨厌的小蚊子，总想钻进我的耳朵里，害我都快痒死了。”狮子离开了大象，心里暗自想着：“原来体型这么巨大的大象，还会怕那么瘦小的蚊子，那我还有什么好抱怨呢？毕竟鸡鸣也不过一天一次，而蚊子却是无时无刻地骚扰着大象。这样想来，我可比他幸运多了。”狮子一边走，一边回头看着仍在跺脚的大象，心想：“天神要我来看看大象的情况，应该就是想告诉我，谁都会遇上麻烦事，而它并无法帮助所有人。既然如此，那我只好靠自己了！反正以后只要鸡鸣时，我就当做鸡是在提醒我该起床了，如此一想，鸡鸣声对我还算是有益处呢？”
      
【温馨提示】

      1个障碍，就是一个新的已知条件，只要愿意，任何一个障碍，都会成为一个超越自我的契机。在人生的路上，无论我们走得多么顺利，但只要稍微遇上一些不顺的事，就会习惯性地抱怨老天亏待我们，进而祈求老天赐给我们更多的力量，帮助我们度过难关。但实际上，老天是最公平的，就像它对狮子和大象一样，每个困境都有其存在的正面价值。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "蜜蜂与蝴蝶",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      蜜蜂在一个地段很好的地方开了一家蜂蜜吧，一开始生意就特别红火。顾客来自各个领域，山上跑的，天上飞的，水里游的。蜂蜜高兴地不停招呼，忙得不亦乐乎。不久，他绞尽脑汁想出了在山坡、水边和森林里开几家分店，把生意做大的好主意。后来他真的又开了好几家分店。
      一天，游乐场的场主蝴蝶从从容容地前来拜访蜜蜂。
      “蜜蜂，出来我们聊聊，你有没有时间啊？”蝴蝶轻松愉快地问。
      蜜蜂又好气又好笑，边团团转地忙碌着，连回答：“我现在忙得连思考‘有没有时间’这个问题的时间都没有了。你没有看到我正忙着开几家蜂蜜吧吗？我至少也要忙完这个周末！”
      “你这不是有时间开几家分店吗？我看你不仅有时间，而且时间多的是，只是没有想问题的时间罢了。”
      蜜蜂听后看看自己，觉得自己就像一只无头苍蝇一样不停地旋转，而蝴蝶在说笑的工夫想出了在蜂蜜吧开一家游乐分场的好主意。
      
【温馨提示】

      蜜蜂忙得连想问题的时间都没有了，其实并不是工作时间越多，你的效率就越高，你获得的利益就越大。学习亦如此，有计划地去学，再忙也要劳逸结合，效率才能提高。记住：别忙得连思考的时间都没有。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "鹿的角和腿",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      从前有一头长着漂亮长角的鹿，来到泉水边喝水，看着水面上的倒影，它不禁洋洋得意。“啊，多么好看的一对长角！”只是，当它看见自己那双似乎细长无力的双腿时，又闷闷不乐了。正在这个时候，出现了一头凶猛的狮子，这头鹿开始拼命地奔跑。由于鹿腿健壮有力，连狮子也被抛得远远的。但到了一片丛林地带之后，鹿角就被树枝绊住了。狮子最后追了上来，一口咬住了它。在临死之时，这头鹿悔恨地说道：“我真蠢！一直不在意的双腿，竟是自己的救命工具，引以自豪的长角，最后竟害了自己！”
      
【温馨提示】

      自己生命中那对华而无益的鹿角和那双坚强有力的鹿腿究竟在哪里呢？",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "为自己而玩",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一群孩子在一位老人家门前嬉闹，叫声连天。几天过去，老人难以忍受。于是，他出来给了每个孩子25美分，对他们说：“你们让这儿变得很热闹，我觉得自己年轻了不少，这点钱表示谢意。”孩子们很高兴，第二天仍然来了，一如既往地嬉闹。老人再出来，给了每个孩子15美分。他解释说，自己没有收入，只能少给一些。15美分也还可以吧，孩子仍然兴高采烈地走了。第三天，老人只给了每个孩子5美分。孩子们勃然大怒，“一天才5美分，知不知道我们多辛苦！”他们向老人发誓，他们再也不会为他玩了！
      
【温馨提示】

      人的动机分两种：内部动机和外部动机。如果按照内部动机去行动，我们就是自己的主人。如果驱使我们的是外部动机，我们就会被外部因素所左右，成为它的奴隶。在这个寓言中，老人的算计很简单，他将孩子们的内部动机“为自己快乐而玩”变成了外部动机“为得到美分而玩”，而他操纵着美分这个外部因素，所以也操纵了孩子们的行为。一个人之所以会形成外部评价体系，最主要的原因是父母喜欢控制他。父母太喜欢使用口头奖惩、物质奖惩等控制孩子，而不去理会孩子自己的动机。久而久之，孩子就忘记了自己的原初动机，做什么都很在乎外部的评价。上学时，他忘记了学习的原初动机——好奇心和学习的快乐；工作后，他又忘记了工作的原初动机——成长的快乐，上司的评价和收入的起伏成了他工作的最大快乐和痛苦的源头。外部评价系统经常是一种家族遗传，但你完全可以打破它，从现在开始培育自己的内部评价体系，让学习和工作变成“为自己而玩”。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "不要跌倒在优势上",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有三个旅行者早上出门时，一个旅行者带了一把伞，另一个旅行者拿了一根拐杖，第三个旅行者什么也没有拿。
      晚上归来，拿伞的旅行者淋得浑身是水，拿拐杖的旅行者跌得满身是伤，而第三个旅行者却安然无恙。于是，前两个旅行者很纳闷，问第三个旅行者：“你怎么没有事呢？”第三个旅行者问拿伞的旅行者：“为什么你淋湿了而没有摔伤呢？”拿伞的旅行说：“下雨的时候，我很高兴有先见之明，撑开伞在雨中大胆地走，衣服还是湿了不少；泥泞难行的地方，因为没有拐杖，所以小心翼翼，就没有跌跤。”然后，他又问拿拐杖的旅行者：“你为什么没有淋湿而摔伤了呢？”拿拐杖的说：“下雨的时候，我因为没有雨伞，便找能躲雨的地方，所以没有淋湿；当我走在泥泞坎坷的路上时，我便用拐杖拄着走，却不知为什么常常跌跤。”第三个旅行者听后笑笑说：“这就是为什么你们拿伞的淋湿了，拿拐杖的跌伤了，而我却安然无恙的原因。当大雨来时我躲着走，当路不好时我细心地走，所以我没有淋湿也没有跌伤。你们的失误就在于你们有凭借的优势，有了优势便少了忧患。”
      
【温馨提示】

      许多时候，我们不是跌倒在自己的缺陷上，而跌倒在自己的优势上。因为缺陷常常给我们以提醒，让我们保持冷静，并不断总结自己该注意的事情。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "树蛙",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      阳光明媚的夏天，一只胸怀大志的青蛙蹲在田埂上，做了一个十分重要的决定：要爬上河边那棵最高的柳树，看看夏天最美丽的风景。知道了这只青蛙爬树的志向后，所有的青蛙都向它投来了羡慕和尊敬的目光，因为它们知道这是一只多么优秀的青蛙，而它们又是多么地信任它。这只青蛙选中位置就往树上跳了，其他所有的青蛙都鼓着眼睛拭目以待。可是，这只青蛙大多是还没抱着树干就掉了下来；即使少数几次抱住了，又不知下一步怎么办，风一起，整个身子晃晃悠悠，一会又掉了下来，但是，这只青蛙还是一次又一次地向自己的目标冲刺着。
      蟋蟀看见这只鼻青脸肿的青蛙，惊奇地问道：“青蛙兄弟，你怎么老是往树上爬呀？”青蛙瞥了蟋蟀一眼，回答道：“我要看夏天美丽的风景，我要改变青蛙不会爬树的命运！”
      燕子飞过来，对青蛙说：“这树是你们青蛙能爬的吗？”青蛙上气不接下气地说：“我想我们能！”
      摔得快死的时候，这只青蛙口里还不停地叫着：“爬树！爬树！”
      所有的青蛙都被这只青蛙的话和坚持不懈的精神深深地感染着，情绪一下子高涨起来；为了实现那只优秀青蛙的遗愿，许多青蛙陆续向河边的柳树跳去，跳去……
      于是，后来地球上有了一个新物种——树蛙。
      
【温馨提示】

      许多时候，我们不是跌倒在自己的缺陷上，而跌倒在自己的优势上。因为缺陷常常给我们以提醒，让我们保持冷静，并不断总结自己该注意的事情。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "能爬上塔顶的青蛙",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      从前，有一群青蛙组织了一场攀爬比赛，比赛的终点是：一个非常高的铁塔的塔顶。一大群青蛙围着铁塔看比赛，给它们加油。比赛开始了，老实说，蛙群中没有谁会相信这些小小的青蛙会到达塔顶，他们都在议论：“这太难了！它们肯定到不了塔顶！”“它们绝不可能成功的，塔太高了！”听到这些话，一只接一只的青蛙开始泄气，除了那些情绪高涨的几只还在往上爬。群蛙继续喊着“这太难了！没有谁能够爬到塔顶的！”
      越来越多的青蛙累坏了，退出了比赛。但只有一只青蛙还在不停地往上爬，一点没有放弃的意思。最后，除了这一只青蛙其它所有的青蛙都退出了比赛，它费了很大的劲，终于成为唯一一只到达塔顶的胜利者。很自然，其他所有青蛙都想知道它是怎么成功的，有一只青蛙跑上去问那只胜利者，它哪里来那么大力气跑完全程？这时，大家发现，它是一只聋子青蛙！！
      
【温馨提示】

      永远不要听信那些习惯消极悲观看问题的人。因为他们只会粉碎你内心的最美好的梦想与希望。总是记住你听到的充满力量的话语，因为所有你听到的或读到的话语都会影响到你的行为。所以，一定要保持积极和乐观！",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "沙漠中走散的骆驼",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      沙漠里有五只骆驼在不停地走啊走，天很热，大家都不知道路的尽头在哪里。因为它们和主人率领的十只骆驼走散了，前面除了黄沙还是黄沙，它们只能凭着最有经验的一只老骆驼的感觉往前走。不一会儿，从它们的右侧方向走出一只精疲力竭的骆驼。原来它是一周前就走散的另一只骆驼。其他骆驼轻蔑地说：“看样子它也不是很精明啊，还不如我们呢！”“是啊，是啊，别理他！免得拖累咱们！”“咱们就装着没看见，它对我们可没有什么帮助！”“看那灰头土脸的样子……”年轻的骆驼你一言我一语，我想避开这只骆驼。老骆驼终于开腔了：“它对我们会很有帮助的！”老骆驼热情地招呼那只落魄的骆驼过来，对它说道：“虽然你也迷路了，境遇比我们好不到哪里去，但是我相信你知道往哪个方向是错误的，这就足够了，和我们一起上路吧！有你的帮助我们会成功的！”
      
【温馨提示】

      我们只不要嘲笑别人的失败，但如果能从别人的失败中总结经验才是明智之举。把别人的失败当成对自己有益的忠告，更有利于我们的成长。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "一条腿的鸭子",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      某城市有个著名的厨师，他的拿手好菜是烤鸭，深受顾客的喜爱，特别是他的老板，更是对其倍加赏识。不过这个老板从来没有给予过厨师任何鼓励，使得厨师整天闷闷不乐。
      有一天，老板有客从远方来，在家设宴招待贵宾，点了数道菜，其中一道是老板最喜欢吃的烤鸭。厨师奉命行事。然而，当老板夹了一只鸭腿给客人时，却找不到另一只鸭腿，他便问身后的厨师说：'另一条腿到哪里去了？'
      厨师说：'老板，我们家里养的鸭子都只有一条腿！'老板感到诧异，但碍于客人在场，不便问个究竟。
      饭后，老板便跟着厨师到鸭笼去查个究竟。时值夜晚，鸭子正在睡觉，每只鸭子都只露出一条腿。
      厨师指着鸭子说：'老板，你看，我们家的鸭子不是全都是只有一条腿吗？'
      老板听后，便大声拍掌，吵醒鸭子，鸭子当场被惊醒，都站了起来。老板说：'鸭子不全是两条腿吗？'
      厨师说：'对！对！不过，只有鼓掌拍手，才会有两条腿呀！'
      
【温馨提示】

      激励奖赏是非常重要的。要经常在公众场所表扬佳绩者或赠送一些礼物给表现特佳者，以资鼓励，激励他们继续奋斗。一点小投资便可换来数倍的业绩，何乐而不为呢？",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "事出必有因",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1、甲：“新搬来的邻居好可恶，昨天晚上三更半夜、夜深人静之时然跑来猛按我家的门”
      乙：“的确可恶！你有没有马上报警？”
      甲：“没有。我当他们是疯子，继续吹我的小喇叭。”
      2、一辆载满乘客的公共汽车沿着下坡路快速前进着，有一个人后面紧紧地追赶着这辆车子。一个乘客从车窗中伸出头来对追车子的人：“老兄！算啦，你追不上的！”“我必须追上它，”这人气喘吁吁地说：“我是这辆车的司机”
      
【温馨提示】

      事出必有因，如果能先看到自己的不是，答案就会不一样。在你面对冲突和争执时，先想一想是否心中有亏，或许很快就能释怀了。）3、某日，张三在山间小路开车。正当他悠哉地欣赏美丽风景时，突然迎面开来一辆货车，而且满囗黑牙的司机还摇下窗户对他大骂一声：“猪！”张三越想越纳闷，也越想越气，于是他也摇下车窗回头大骂：“你才是猪！”才刚骂完，他便迎头撞上一群过马路的猪。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "比尔?盖茨给青少年的11条准则",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      “许多人都以为生活是由偶然和运气组成的，其实不然，它是由规律和法规组成的。”《比尔?盖茨给青少年的11条准则》就是盖茨先生从自己生活的方方面面，以及他从小到大的个人经历中总结出来的成功经验和人生智慧。让我们一起来分享这一“财富背后的财富”。
      第一条准则：适应生活
      生活是不公平的，要去适应它。命运掌握在自己手中。
      第二条准则：成功是你的人格资本
      这世界并不会在意你的自尊。这世界指望你在自我感觉良好之前先要有所成就。成功是人生的最高境界，成功可以改变你的人格和尊严，自负是愚蠢的。
      第三条准则：别希望不劳而获
      成功不会自动降临，成功来自积极的努力，要分解目标，循序渐进，坚持到底。
      第四条准则：习惯律己
      如果你认为你的老师严厉，等你当了老板再这样想，老板可是没有任期限制的。好习惯源于自我培养。
      第五条准则：不要忽视小事
      烙牛肉饼并不有损你的尊严，平凡成就大事业。
      第六条准则：从错误中吸取教训
      如果你陷入困境，不要尖声抱怨，要从中吸取教训。
      第七条准则：事事自己动手
      在你出生之前，你的父母并非像他们现在这样乏味。他们变成今天这个样子是因为这些年来他们一直在为你付账单，给你洗衣服，听你大谈你是如何的酷。不要总靠别人活着，要凭借自己的力量前进。
      第八条准则：你往往只有一次机会
      你的学校也许已经不再分优等生和劣等生，但生活却仍在做出类似区分。在某些学校已经废除不及格分，只要你想找到正确答案，学校就会给你无数的机会。这和现实生活中的任何事情没有一点相似之处。机遇是一种巨大的财富，机遇往往就那么一次，也许你“没有机会”，但可以创造。
      第九条准则：时间，在你手中
      生活不分学期，你并没有暑假可以休息，也没有几位雇主乐于帮你发现自我。自己找时间做吧，决不要把今天的事情拖到明天。
      第十条准则：做该做的事
      电视并不是真实的生活。在现实生活中，人们实际上得离开咖啡屋去干自己的工作。
      第十一条准则：善待身边的人
      善待他人就是善待自己，要用赞扬代替批评并主动适应对方。
      
【温馨提示】

      事出必有因，如果能先看到自己的不是，答案就会不一样。在你面对冲突和争执时，先想一想是否心中有亏，或许很快就能释怀了。）3、某日，张三在山间小路开车。正当他悠哉地欣赏美丽风景时，突然迎面开来一辆货车，而且满囗黑牙的司机还摇下窗户对他大骂一声：“猪！”张三越想越纳闷，也越想越气，于是他也摇下车窗回头大骂：“你才是猪！”才刚骂完，他便迎头撞上一群过马路的猪。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "天才的成功不相信结局",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      老希尔顿创建希尔顿旅店帝国时，曾指天发誓：“我要使每一寸土地都生长出黄金来。”
      故事发生在70年前，希尔顿以700万美金买下华尔道夫——阿斯托里亚大酒店的控制权后，以极快的速度接管了这家纽约著名的宾馆。一切欣欣向荣，开始进入正常的营运状态。所有的经理们都以为已经充分利用了一切生财手段。但老希尔顿却一言不发地查找着可能没有充分利用的空间。他的脚步开始在总服务台前停顿，他注视着大厅中央那些巨大的通天圆柱。既然这四个空心圆柱在建筑结构上没有支撑天花板的力学作用，那么，它们还有什么存在的意义。于是，他叫人把它们迅速改造成四个透明的玻璃柱，并在其中设置了漂亮的系列玻璃展箱。这回四根圆柱就不仅仅是装饰性的了，还充满了商业意义。没几天，纽约的珠宝商和香水制造厂家便把它们全部包租下来，并把自己的产品摆了进去。而希尔顿坐享其成，每年由此净收入20万美元的租金。
      
【温馨提示】

      天才是不相信结局的。因为有时结局只不过是人们为自己休息而找的借口。既然结局并不存在，我们就应该放弃种种借口，继续往前走。",
            group4));

            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
            "责任感能够创造奇迹",
            "Item Subtitle: 3",
            "Assets/title/4.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      几年前，美国著名心理学博士艾尔森对世界100名各个领域中杰出人士做了问卷调查，结果让他十分惊讶—其中61名杰出人士承认，他们所从事的职业，并不是他们内心最喜欢做的，至少不是他们心目中最理想的。这些杰出人士竟然在自己并非喜欢的领域里取得了那样辉煌的业绩，除了聪颖和勤奋之外，究竟靠的是什么呢？带着这样的疑问，艾尔森博士又走访了多位商界英才。其中纽约证券公司的金领丽人苏珊的经历，为他寻找满意的答案提供了有益的启示。
      苏珊出身于中国台北的一个音乐世家，她从小就受到了很好的音乐启蒙教育，非常喜欢音乐，期望自己的一生能够驰骋在音乐的广阔天地，但她阴差阳错地考进了大学的工商管理系。一向认真的她，尽管不喜欢这一专业，可还是学得格外刻苦，每学期各科成绩均是优异。毕业时被保送到美国麻省理工学院，攻读当时许多学生可望而不可及的MBA，后来，她又以优异的成绩拿到了经济管理专业的博士学位。如今她已是美国证券业界风云人物，在被调查时依然心存遗憾地说：“老实说，至今为止，我仍不喜欢自己所从事的工作。如果能够让我重新选择，我会毫不犹豫地选择音乐。但我知道那只能是一个美好的‘假如’了，我只能把手头的工作做好……”艾尔森博士直截了当地问她：“既然你不喜欢你的专业，为何你学得那么棒？既然不喜欢眼下的工作，为何你又做得那么优秀？”苏珊的眼里闪着自信，十分明确地回答：“因为我在那个位置上，那里有我应尽的职责，我必须认真对待。”“不管喜欢不喜欢，那都是我自己必须面对的，都没有理由草草应付，都必须尽心尽力，尽职尽责，那不仅是对工作负责，也是对自己负责。有责任感可以创造奇迹。”
      艾尔森在以后的继续的走访中，许多的成功人士之所以能出类拔萃的反思，与苏珊的思考大致相同—因为种种原因，我们常常被安排到自己并不十分喜欢的领域，从事了并不十分理想的工作，一时又无法更改。这时，任何的抱怨、消极、懈怠，都是不足取的。唯有把那份工作当作一种不可推卸的责任担在肩头，全身心地投入其中，才是正确与明智的选择。正是这种“在其位，谋其政，尽其责，成其事”的高度责任感的驱使下，他们才赢得了令人瞩目的成功。
      从艾尔森博士的调查结论，使人想到了我国的著名词作家乔羽。最近，他在中央电视台艺术人生节目里坦言，自己年轻时最喜欢做的工作不是文学，也不是写歌词，而是研究哲学或经济学。他甚至开玩笑地说，自己很可能成为科学院的一名院士。不用多说，他在并非最喜欢和最理想的工作岗位上兢兢业业，为人民做出了家喻户晓、人人皆知的贡献。
      
【温馨提示】

      “热爱是最好的教师”，“做自己想做的事”，这些话已经是句耳熟能详的名言。但是，“责任感可以创造奇迹”，却容易被人忽视。对许多杰出人士的调查说明，只要有高度的责任感，即使在自己并非最喜欢和最理想的工作岗位上，也可以创造出非凡的奇迹。",
            group4));



            this.AllGroups.Add(group4);
            #endregion

            #region 第5章
            var group5 = new SampleDataGroup("Group-5",
            "第5章",
            "Group Subtitle: 5",
            "Assets/title/5.png",
            "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
"安然总裁的故事",
"Item Subtitle: 3",
"Assets/title/5.png",
"Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
@"
      一个城里男孩Kenny移居到了乡下，从一个农民那里花100美元买了一头驴，这个农民同意第二天把驴带来给他。第二天农民来找Kenny，说：“对不起，小伙子，我有一个坏消息要告诉你，那头驴死了。”Kenny回答：“好吧，你把钱还给我就行了！”农民说：“不行，我不能把钱还给你，我已经把钱给花掉了。”Kenny说：“ok，那么就把那头死驴给我吧！”农民很纳闷：“你要那头死驴干嘛？”Kenny说：“我可以用那头死驴作为幸运抽奖的奖品。”农民叫了起来：“你不可能把一头死驴作为抽奖奖品，没有人会要它的。”Kenny回答：“别担心，看我的。我不告诉任何人这头驴是死的就行了！”
      几个月以后，农民遇到了Kenny,农民问他：“那头死驴后来怎么样了？”Kenny说：“我举办了一次幸运抽奖，并把那头驴作为奖品，我卖出了500张票，每张２块钱，就这样我赚了998块钱！”农民好奇地问：“难道没有人对此表示不满？”Kenny回答：“只有那个中奖的人表示不满，所以我把他买票的钱还给了他！”
      许多年后，长大了的Kenny成为了安然公司的总裁。
      
【温馨提示】

      方法总比问题多，不同方式对待同一个难题就有不同的结果。",
group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "愿望与成功之间",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1865年，美国南北战争结束了。一位叫马维尔的记者去采访林肯，他们有这么段对话。
      马维尔：“据我所知，上两届总统都曾想过废除黑奴制，《解放黑奴宣言》也早在他们那个时期就已草就，可是他们都没拿起笔签署它，请问总统先生，他们是不是想把这一伟业留下来，给您去成就英名？”
      林肯：“可能有这个意思吧。不过，如果他们知道拿起笔需要的仅是一点勇气，我想他们一定非常懊丧。”
      马维尔还没来得及问下去，林肯的马车就出发了，因此，他一直都没弄明白林肯的这句话到底是什么意思。
      直到林肯去世了，马维尔才在林肯致朋友的一封信中找到答案。在信里，林肯谈到幼年的一段经历：“我父亲在西雅图有一处农场，上面有许多石头。正因如此，父亲才得以较低价格买下它。有一天，母亲建议把上面的石头搬走。父亲说，如果可以搬走的话，主人就不会卖给我们了，它们是一座座小山头，都与大山连着。”有一年，父亲去城里买马，母亲带我们在农场劳动。母亲说：“让我们把这些碍事的东西搬走，好吗？”于是我们开始挖那一块块石头。不长时间，就把它们弄走了，因为它们并不是父亲想象的山头，而是一块块孤零零的石块，只要往下挖一英尺，就可以把它们晃动。
      
【温馨提示】

      有些事情人们之所以不去做，只是他们认为不可能。而许多不可能，只存在于人的想象之中。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "永不退缩的林肯总统",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      生下来就一贫如洗的林肯，终其一生都在面对挫败，八次竞选八次落败，两次经商失败，甚至还精神崩溃过一次。然而面对这些，他并没有放弃，最终成为美国历史上最伟大的总统之一。
      以下是林肯进驻白宫前的简历：
      1831年，经商失败。
      1832年，竞选州议员——但落选了！
      1832年，工作也丢了——想就读法学院，但进不去。
      1833年，向朋友借钱经商，但年底就破产了，后花了16年才把债还清。
      1835年，再次竞选州议员——赢了！
      1835年，订婚后即将结婚时，未婚妻却死了，因此他的精神完全崩溃，卧病在床六个月。
      1840年，争取成为选举人——失败了！
      1843年，参加国会大选——落选了！
      1848年，寻求国会议员连任——失败了！
      1849年，想在自己的州内担任土地局长的工作——被拒绝了！
      1854年，竞选美国参议员——落选了！
      1858年，再度竞选美国参议员——再度落败。
      1860年，当选美国总统。
      
【温馨提示】

      失败是成功的必由之路，失败越多，离成功也就越近；那些永远不敢尝试、害怕失败的人永远不能获得成功。偶尔的失败千万不能自暴自弃自卑自贱，一旦你放手你将什么也没有，但你继续斗争下去，总会有转机出现。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "养成让自己进步的习惯",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      摘自<<牛津管理评论>>,养成让自己进步的26个习惯:
      01.永远不说三个字“不可能”。
      02.凡事第一反应是找方法，而非是找借口
      03.遇到挫折时大声对自己说：“太棒了，我终于有机会成长了。”
      04.不说消极的话，不落入消极的情绪当中，一旦出现问题应正面处理。
      05.凡事先定订目标，并尽量制作梦想版。
      06.凡事预先做计划，尽量将目标视觉化。
      07.是工作和学习的时间，就要全部的用在工作和学习上，不要盗用工作和学习的时间。
      08.养成记录的习惯，不要太依赖自己的脑袋记忆。
      09.随时记录灵感。
      10.把重要的观点，目标，方法写下来，并贴出来，随时提醒自己。
      11.走路比平时快30%，走路是脚尖用力向前推进，肢体语言健康有力，不懒散，不颓废。
      12.每天出门照镜子，给自己一个自信的微笑。
      13.每天自我反省一下，自检一下。
      14.开会时坐前排。
      15.时时刻刻微笑待人处事。
      16.用心倾听，不打断别人的话，作一个倾听高手。
      17.说话声声有力，能感染自己，能产生磁场。
      18.同理心，说话之前先考虑对方的感受。
      19.每天有意识的真诚的赞美别人三次以上。
      20.及时写感谢卡。
      21.用关心和自责的口吻说话，责人之前先责己。
      22.每天进步一点点，日有一新，月有一进，每天多做一件事。
      23.提前上班，推迟下班。
      24.节俭并定期存钱。
      25.时常运用头脑风暴。
      26.遵守诚信，说到做到。
      
【温馨提示】

      失败是成功的必由之路，失败越多，离成功也就越近；那些永远不敢尝试、害怕失败的人永远不能获得成功。偶尔的失败千万不能自暴自弃自卑自贱，一旦你放手你将什么也没有，但你继续斗争下去，总会有转机出现。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "成功不可复制",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      这是一位少年的有趣经历：
      1、6岁时，一位非洲的主教跟他一块儿玩了一下午的滚球，他觉得从来没有一位大人对他这么好过，认为黑人是最优秀的人种。
      2、8岁那年，他有了一个嗜好，喜欢问父亲的朋友有多少财产，大部分人都被他吓了一跳，只好昏头昏脑地告诉他。
      3、上小学时，他常常花一整天时间偷看大姐的情书，从来没有被发觉。
      4、他天生哮喘，夜里总是辗转难眠，白天又异常疲惫，这个病一直折磨着他。他对很多东西都有恐惧症，比如大海。
      5、他恳求父亲带他去钓鱼，父亲说：“你没有耐心，带你去你会把我弄疯的。”也由于没有耐性，他成了牛津大学的肄业生。
      6、老师问他拿破仑是哪国人，他觉得有诈，自作聪明的改以荷兰人作答，结果遭到了不准吃晚饭的惩罚。
      7、他总觉得自己的智商只比天才低一点，结果一测试，只有96，只是普通人的正常智商。
      下面，我们再来看一位伟大人物的传奇：
      1、他一生朋友无数，他曾列了一个有50个名字的挚友清单，包括美国国防部部长、纽约的著名律师、报刊总编以及女房东、农场的邻居、贫民区的医生等等。
      2、二战期间，在他31岁时，他为了帮助自己的祖国，服务于英国情报局，当了几年的间谍。
      3、38岁时，他记起祖父从一个失败的农夫成为一名成功的商人，于是决定效仿。没有文凭的他，以6000美元起家，创办了全球最大的广告公司，年营业额达数十亿美元。
      4、他曾自嘲：“只要比竞争对手活得长，你就赢了。”他活了88岁。
      5、他一生都在冒险，大学没读完，就跑到巴黎当厨师，继而卖厨具，到美国好莱坞做调查员，随后又作了间谍、农民和广告人。晚年隐居于法国古堡。
      6、他敢于想象，设计了无数优秀的广告词，至今仍在使用。
      7、他说：“永远不要把财富和头脑混为一谈，一个人赚很多钱和他的头脑没有多大关系。”
      那位少年和伟人是同一个人——大卫?奥格威，奥美广告公司创始人。
      我们把上述两对7个例子一一对应，便会发现它们之间没有所谓成功的必然规律：有的可以牵强的联系起来，比如偷看情书为当间谍作了铺垫，对财富的欲望导致日后开了广告公司，天性友善适合结交朋友；有的则完全相反，没有耐性却创造了伟业，身体不好却长寿，智商不高却有着惊人的智慧。当然，我们也可以不一一对应。可是，你看了这位少年的有趣经历一定能断定他会成为伟大人物吗？
      大卫?奥格威的成功在于他顺从了性格，并将自己的特点(优点)发挥得淋漓尽致。
      
【温馨提示】

      成功是不可复制的，每个人都有自己的成功方式，人的性格、环境、智商、情商、机遇、身份都不一样，怎能拷贝成功？如果说成功有规律可循，那么便是认识自己、创造自己、成为自己。记住：“市场永远不变的法则就是永远在变！”、“估量命运的秘诀就是不可估量！”",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "政治家和商人的生活原则",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      政治家生活原则
      托马克斯?杰斐逊是美国第三任总统，他在给孙子的忠告里，提到了以下10点生活原则：
      1．今天能做的事情绝对不要推到明天。
      2．自己能做的事情绝对不要麻烦别人。
      3．决不要花还没有到手的钱。
      4．决不要贪图便宜购买你不需要的东西。
      5．绝对不要骄傲，那比饥饿和寒冷更有害。
      6．不要贪食，吃得过少不会使人懊悔。
      7．不要做勉强的事情，只有心甘情愿才能把事情做好。
      8．对于不可能发生的事情不要庸人自忧。
      9．凡事要讲究方式方法。
      10．当你气恼时，先数到10再说话，如果还气恼，那就数到100。
      商人生活原则
      约翰?丹佛是美国硅谷著名的股票经纪人，也是有名的亿万富翁，在对记者的一次答辩中，他也发表了对以上几个问题的看法。从鲜明的对比中，我们可以看出一个政治家和一个商人的截然不同。
      1．今天能做的事情如果放到明天去做，你就会发现很有趣的结果。尤其是买卖股票的时候。
      2．别人能做的事情，绝不自己动手去做。因为我相信，只有别人做不了的事情才值得我去做。
      3．如果可以花别人的钱来为自己赚钱，我就绝对不从自己的口袋里掏一个子儿。
      4．我经常在商品打折时候去买很多东西，哪怕那些东西现在用不着，可是总有用得着的时候，这是一个预测功能。就像我只在股票低迷的时候买进，需要的是同样的预测功能。
      5．很多人认为我是一个狂妄自大的人，这有什么不对吗？我的父母、我的朋友们在为我骄傲，我看不出我有什么理由不为自己骄傲，我做得很好，我成功了。
      6．我从来不认为节食这么无聊的话题有什么值得讨论的。哪怕是为了我们的营养专家们高兴，我也要做出喜欢美食的样子，事实上，我的确喜欢美妙的食物，我相信大多数的人有跟我一样的喜好。
      7．我常常不得不做我不喜欢的事情。我想在这个世界上，我们都没在办法完全按照自己的意愿做事。正像我的理想是一个音乐家，最后却成为一个股票经纪人一人样。
      8．我常常预测灾难的发生，哪怕那个灾难的可能性在别人看来几乎为零。正是我的这种动物的本能使我的公司在美国的历金融危机中逃生。
      9．我认为只要目的的确定，就要不惜代价去实现它。至于手段，在这个时代，人们只重视结果，有谁去在乎手段呢？
      10．我从不隐瞒我的个人爱好，以及我对一个人的看法，尤其是当我气恼的时候，我一定要用大声吼叫的方式发泄出来。
      
【温馨提示】

      不同的行业，不同的人，有不同的生活方式和做人原则，进一步讲就是，成功没在固定的模式，一味模仿别人不可能取得大的成就。所以，我们在学习进程中总结出属于我们自己的成功模式。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "生命的价值",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在一次讨论会上,一位著名的演说家没讲一句开场白,手里却高举着一张20美元的钞票.面对会议室里的200个人,他问:'谁要这20美元?'一只只手举了起来.他接着说:'我打算把这20美元送给你们中的一位,但在这之前,请准许我做一件事.'他说着将钞票揉成一团,然后问:'谁还要?'仍有人举起手来.他又说:'那么,假如我这样做又会怎么样呢?'他把钞票扔到地上,又踏上一只脚,并且用脚碾它.尔后他拾起钞票,钞票已变得又脏又皱.'现在谁还要?'还是有人举起手来.'朋友们,你们已经上了一堂很有意义的课.无论我如何对待那张钞票,你们还是想要它,因为它并没贬值,它依旧值20美元.人生路上,我们会无数次被自己的决定或碰到的逆境击倒,欺凌甚至碾得粉身碎骨.我们觉得自己似乎一文不值.但无论发生什么,或将要发生什么,在上帝的眼中,你们永远不会丧失价值.在他看来,肮脏或洁净,衣着齐整或不齐整,你们依然是无价之宝.'
      
【温馨提示】

      生命的价值不依赖我们的所作所为,也不仰仗我们结交的人物,而是取决于我们本身!我们是独特的——永远不要忘记这一点!",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "三只老鼠",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      三只老鼠一同去偷油喝。找到了一个油瓶，三只老鼠商量，一只踩着一只的肩膀，轮流上去喝油。于是三只老鼠开始叠罗汉，当最后一只老鼠刚刚爬到另外两只的肩膀上，不只什么原因，油瓶倒了，最后，惊动了人，三只老鼠逃跑了。回到老鼠窝，大家开会讨论为什么会失败。最上面的老鼠说：“我没有喝到油，而且推倒了油瓶，是因为下面第二只老鼠抖动了一下，所以我推倒了油瓶。”第二只老鼠说：“我抖了一下，但我感觉到第三只老鼠也抽搐了一下，我才抖动了一下。”第三只老鼠说：“对，对，我因为好像听见门外有猫的叫声，所以抖了一下。”“哦，原来如此呀！”
      
【温馨提示】

      我们都不能把所谓的客观因素当原因，责任人人有份，许多问题的解决都不会是“自古华山一条道”，每个人应该都多少能找到解决之道，这就是主观态度问题了。推诿只是表面现象。主要是素质问题。1、没有团队精神；2、没有责任心；3、没有面对错误的勇气；4、不能反求诸己；5、凡事归罪于外......",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "一位父亲给儿子的信",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      洛克菲勒给儿子的信：现在就去做。
      亲爱的约翰：
      聪明人说的话总能让我记得很牢。有位聪明人说得好，“教育涵盖了许多方面，但是他本身不教你任何一面。”这位聪明人向我们展示了一条真理：如果你不采取行动，世界上最实用、最美丽、最可行的哲学也无法行得通。
      我一直相信，机会是靠机会得来的。再好的构想都有缺陷，即使是很普通的计划，但如果确实执行并且继续发展，都会比半途而废的好计划要好得多，因为前者会贯彻始终，后者却前功尽弃。所以我说，成功没有秘诀，要在人生中取得正面结果，有过人的聪明智慧、特别的才艺当然好，没有也无可厚非，只要肯积极行动，你就会越来越接近成功。
      遗憾的是，很多人并没有记取这个最大的教训，结果将自己沦为了平庸之辈。看看那些庸庸碌碌的普通人，你就会发现，他们都有在被动的活着，他们说的远比做的多，甚至只说不做。但他们几乎个个都是找借口的行家，他们会找各种借口来拖延，直到最后他们证明这件事不应该、没有能力去做或已经来不及了为止。
      与这类人相比，我似乎聪明、狡猾了许多。盖茨先生吹捧我是个主动做事、自动自发的行动者。我很乐意这样的吹捧，因为我没有辜负它。积极行动是我身上的另一个标识，我从不喜欢纸上谈兵或流于空谈。因为我知道，没有行动就没有结果，世界上没有哪一件东西不是由一个个想法付诸实施所得来的。人只要活着，就必须考虑行动。
      很多人都承认，没有智慧的基础的知识是没用的，但更令人沮丧的是即使空有知识和智慧，如果没有行动，一切仍属空谈。行动与充分准备其实可视为物体的两面。人生必须适可而止。做太多的准备却迟迟不去行动，最后只会徒然浪费时间。换句话说，事事必须有节制，我们不能落入不断演练、计划的圈套，而必须承认现实：不论计划有多周详，我们仍然不可能准确预测最后的解决方案。
      我当然不否认计划非常重要，计划是获得有利结果的第一步，但计划并非行动，也无法代替行动。就如同打高尔夫球一样，如果没有打过第一洞，便无法到达第二洞。行动解决一切。没有行动，什么都不会发生。我们无论如何也买不到万无一失的保险，但我们可以做到的是下定决心去实行我们的计划。
      缺乏行动的人，都有一个坏习惯：喜欢维持现状，拒绝改变。我认为这是一种深具欺骗和自我毁灭效果的坏习惯，因为一切都在变化之中，正如人会生死一样，没有不变的事物。但因内心的恐惧——对未知的恐惧，很多人抗拒改变，哪怕现状多么不令他满意，他都不敢向前跨出一步。看看那些本该事业有成，却一事无成的人，你就知道不同情他们是件很难的事。
      是的，每个人在决定一件大事时，心里都会或多或少有些担心、恐惧，都会面对到底要不要做的困扰。但“行动派”会用决心燃起心灵的火花，想出各种办法来完成他们地心愿，更有勇气克服种种困难。
      很多缺乏行动的人大都很天真，喜欢坐等事情自然发生。他们天真地以为，别人会关心他们的事。事实上，除了自己以外，别人对他们不大感兴趣，人们只对自己的事情感兴趣。例如一桩生意，我们获利比重越高，就要越主动采取行动，因为成败与别人的关系不大，他们不会在乎的。这时候，我们最好把它推一把，如果我们怠惰、退缩，坐等别人采取主动来推动事情的话，结果必定会令人失望。
      一个人只有自己依靠自己，他才不会让自己失望，并能增加自己控制命运的机会。聪明人只会去促使事情发生。
      人生中最令人感到挫折的，莫过于想做的事太多，结果不但没有足够的时间去做，反而想到每件事的步骤繁多，而被做不到的情绪所震慑，以致一事无成。我们必须承认，时间有限，任何人都无法做完所有的事情。聪明人知道，并非所有的行动都会产生好的结果，只有明智的行动才能带来有意义的结果，所以聪明人只会汲取做了以后获得正面效果的工作，做与完成最大目标有关的工作，而且专心致志，所以聪明人总能做出最有价值的贡献，并捞到很多好处。
      要吃掉大象需要一口一口的吃，做事也是一样，想完成所有的事情，只会让机会溜掉。我的座右铭是：洛克菲勒对紧急事件采取不公平待遇。
      很多人都是自己使自己变成一个被动者的，他们想等到所有的条件都十全十美，也就是时机对了以后才行动。人生随时都是机会，但是几乎没有十全十美的。那些被动的人平庸一辈子，恰恰是因为他们一定要等到每一件事情都百分之百的有利，万无一失以后才去做。这是傻瓜的做法。我们必须向生命妥协相信手上的正是目前需要的机会，才会将自己挡在陷入行动前永远痴痴等待的泥沼之外。
      我们追求完美，但是人类的事情没有一件绝对完美，只有接近完美。等到所有条件都完美以后才去做，只能永远等下去，并将机会拱手让给他人。那些要等到所有事情都已经准备妥当才出发的人，将永远也离不开家。要想变成“我现在就去做”的那种人，就是停止一切白日梦，时时想到现在，从现在就开始做。诸如“明天”、“下礼拜”、“将来”之类的句子，跟“永远不可能做到”意义相同。
      每个人都有失去自信，怀疑自己能力的时候，尤其是在逆境中的时候。但真正懂得行动艺术的人，却可以用坚强的毅力克服它，会告诉自己每个人都有失败的时候，有失败得很惨的时候，会告诉自己不论事前做了多少准备、思考多久，真正着手做的时候，都有难免会犯错误。然而，被动的人，并不把失败视为学习和成长的机会，却总在告诫自己：或许我真的不行了，以致失去了积极参与未来的行动。
      很多人都相信心想事成，但我却将其视为慌言。好主意一毛钱能买一打，最初的想法只是一连串行动的起步，接下来需要第二阶段的准备、计划和第三阶段的行动。在我们这个世界上从来不缺少有想法有主意的人，但懂得成功地将一个好主意付诸实现比在家空想出一千个好主意要有价值得多的人却很少。
      人们用来判断你的能力的真正基础，不是你脑子里装了多少东西，而是你的行动。人们都信任脚踏实地的人，他们都会想：这个人敢说敢做，一定知道怎么做最好。我还没听过有人因为没有打扰别人、没有采取行动或要等别人下令才做事而受到赞扬的。那些在工商界、政府、军队中的领袖，都是很能干又肯干的人、百分之百主动的人。那些站在场外袖手旁观的人永远当不成领导人物。
      不论是自动自发者还是被动的人，都是习惯使然。习惯有如绳索，我们每天纺织一根绳索，最后它粗大得无法折断。习惯的绳索不是带领我们到高峰就是引领我们到低谷，这主得看好习惯或坏习惯了。坏习惯能摆布我们、左右成败，它很容易养成，但却很难伺候。好习惯很难养成，但很容易维持下去。
      要有现在就做的习惯，最重要的是要有积极主动的精神，戒除精神散漫的习惯，要决心做个主动的人，要勇于做事，不要等到万事俱备以后才去做，永远没有绝对完美的事。培养行动的习惯，不需要特殊的聪明智慧或专门的技巧，只需要努力耕耘，让好习惯在生活中开花结果即可。
      儿子，人生就是一场伟大的战役，为了胜利，你需要行动，再行动，永远行动！这样，你的安全就能得到保障。
      我想没有比在此时送给你这封信更好的礼物了。
      
【温馨提示】

      我们都不能把所谓的客观因素当原因，责任人人有份，许多问题的解决都不会是“自古华山一条道”，每个人应该都多少能找到解决之道，这就是主观态度问题了。推诿只是表面现象。主要是素质问题。1、没有团队精神；2、没有责任心；3、没有面对错误的勇气；4、不能反求诸己；5、凡事归罪于外......",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "黑、白石头",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      从前，在欠债不还便足以使人入狱的时代，伦敦有位商人，欠了一位放高利贷的债主一笔巨款。那个又老又丑的债主，看上商人青春美丽的女儿，便要求商人用女儿来抵债。商人和女儿听到这个提议都十分恐慌。狡猾伪善的高利贷债主故作仁慈，建议这件事听从上天安排。他说，他将在空钱袋里放入一颗黑石子，一颗白石子，然后让商人女儿伸手摸出其一，如果她拣中的是黑石子，她就要成为他的妻子，商人的债务也不用还了；如果她拣中的是白石子，她不但可以回到父亲身边，债务也一笔勾销；但是，假如她拒绝探手一试，她父亲就要入狱。虽然是不情愿，商人的女儿还是答应试一试。当时，他们正在花园中铺满石子的小径上，协议之后，高利贷的债主随即弯腰拾起两颗小石子，放入袋中。敏锐的少女突然察觉：两颗小石子竟然全是黑的！女孩不发一语，冷静的伸手探入袋中，漫不经心似的，眼睛看着别处，摸出一颗石子。突然，手一松，石子便顺势滚落路上的石子堆里，分辨不出是那一颗了。
      「噢！看我笨手笨脚的，」女孩呼道「不过，没关系，现在只需看看袋子里剩下的这颗石子是什么颜色，就可以知道我刚才选的那一颗是黑是白了。」
      当然啰，袋子剩下的石子一定是黑的，恶债主既然不能承认自己的诡诈，也就只好承认她选中的是白石子。一场债务风波，有惊无险的落幕。
      
【温馨提示】

      这不是『解决导向』的思考模式所能处理的，因为，水平思考方式不把关键摆在选出的石子，而是换一个角度来看，『袋子里剩下来的石子是什么颜色？』，终于逢凶化吉，把最险恶的危机变成最有利的情况。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "你用不着跑在任何人后面",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      理查?派克是运动史上赢得奖金最多的赛车选手。他第一次赛车回来时，兴奋地对母亲说：“有35辆车参赛，我跑了第二。”“你输了！”母亲毫不客气地回答。“可是，”理查?派克瞪大了眼睛，“这是我第一次参加比赛，而且赛车还这么多。”“儿子，”母亲深情地说，“记住，你用不着跑在任何人后面！”
      接下来的20年中，理查?派克称霸赛车界。他的许多记录至今无人打破。问他成功的原因，他说，他从未忘记母亲的教诲，是母亲在他为第二名沾沾自喜之时，帮他发现了他还可能是第一的希望。
      第一是人们梦寐以求的，这个世界上也不可能所有的人都争得第一，可是，试想一下理查?派克，如果他连第一都不敢想，他连自己都不自信，如果他得不到母亲深情的鼓舞，他能在20年的时间里称霸赛车世界吗？
      
【温馨提示】

      母亲是一个容器，她能决定孩子心胸的大小；母亲是一片天空，她能决定孩子前程的远近。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "从猫晒太阳说起",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      尼里斯?劳津是丹麦的一位医生。一天，他在屋里看书，看久了，眼睛有些疲倦，就走到窗前去“放松”一下。正巧，院子里有只猫躺在地上晒太阳。真有趣，太阳一分一秒地向西移动，那树影儿也跟着一点点朝东移动，眼看就要遮住猫的身体了，那猫立刻挪动一下身子，树阴每移动一步，猫也跟着挪动一步，始终不让树阴遮住自己。这是为什么呢？这会儿天气还不算太冷，难道猫就这么喜欢晒太阳？劳津决定弄个明白。他索性走到院子里，蹲在猫的身边，仔细地观察起来。这一下他完全弄明白了：原来这只猫身上有个流脓的伤口。有好几天，它都爱躺在院子里晒太阳。不出几天，这伤口就全好了。劳津是个医生，他立刻从猫想到了人。太阳光能促使猫的伤口尽早愈合，会不会也帮助人治疗伤病呢？带着这个问题，劳津做了一系列的实验。后来他果然写出了《光对人体的生理作用》等研究论文，获得了世界科学的最高荣誉——诺贝尔奖。
      猫晒太阳，这是一件多么平常的小事啊！换了别人，谁也不会当回事。惟独到了劳津医生的眼里，却成了一项重大的发现。因为他有一双特别的眼睛一份特别的好奇心，能够及时捕捉住这瞬间的秘密，并进而深入研究，从而摘取了诺贝尔奖。
      这就是出色的观察力。这就是非凡的观察力。只有具有非凡观察力的人，才能成就非凡的事业。
      哥白尼站在一望无际的大海边，久久地凝视着海平线上出海归来的帆船：为什么总是最先看到那高高竖起的桅杆，而后才一点点看见船身？哦，这还不明白吗，地球是圆的！从而彻底颠覆了那本一直被奉为经典的《基督教宇宙地形学》中所说的谬论：地球是个“长方形的箱子”，“大地是它的箱底，天空是他的盖子”。
      伽利略坐在肃静的比萨教堂里，抬头注视着随风摆动的屋顶吊灯：风大，摆动幅度大，风小，摆动幅度小，可为什么每次摆动的时间都一样？应用这一原理，不是就可以计算时间了吗？于是就有了带摆时钟的发明。
      牛顿从对苹果落地的观察中引发对万有引力的思考，由弟弟妹妹的吹肥皂泡游戏感悟到太阳光的七色秘密……
      德国科学家魏格纳因因病卧床不起，无聊中看看世界地图，却推出了一个崭新的理论：大陆板块漂移说。
      还有一个笑话：有位教授，为了证实糖尿病病人的尿样中含有糖份，他把一只手指伸进一杯事先准备好的尿样中。他先做了一个示范动作：把蘸了尿液的指头放在自己的舌头上尝了一下。他做完后要求学生们也照样试验一遍。学生们都皱起了眉头，虽然老大不愿意，但还是一个个照着老师的榜样把手指伸进尿液，然后又急急忙忙地用舌头去舔了舔。教授摇摇头，露出哭笑不得的表情。他十分遗憾地说：“你们为了科学，甘愿亲身体验的精神，我深表赞赏。但是若以你们这种粗心大意的观察力去从事科学工作，将来想有非凡作为，取得出色成就，我只能说——NO！”原来，教授在做实验时，使了个小小的花招：他伸进尿液时用的是食指，而放在舌头上的却是中指，只是动作做得比较快，骗过了大家的眼睛而已。请不要把它仅仅当作一则笑话。
      
【温馨提示】

      天才和普通人问问题的方式没什么区别，只是他们会多想一步，让思维拐个弯。多问几个为什么很容易，难得是继续找出问题答案的精神。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "过去不代表将来",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个15岁的孩子，他顽皮，粗鲁，无礼。在学校里，他惹事生非，与同学打架，没有人喜欢他，也很少有人和他在一起玩耍。但有一个人除外，那是一个叫菲拉的女教师。一天放学后，菲拉叫住了孩子，说：“是这样子的，老师最近遇到了一道难题，你能帮老师找出答案吗？”有三个候选人，他们分别是：1.有点迷信，有两个情妇，抽烟凶狠，并且嗜酒如命；2.恋床一般到中午才起来，曾经吸过鸦片；3.曾是国家的战斗英雄，不吸烟，偶尔喝点啤酒，年轻时从未做过违法的事情。菲拉说：“假如上帝要在这三个人中选一位在后来能够造福人类的人，那么他老人家会选谁呢？”孩子想都没多想，就说：“当然是第三个人啦。”“不对，孩子。”菲拉摇摇头说：“你知道第三个人是谁吗？他的全名叫阿道夫-希特勒，他的罪行遭到全世界人民的唾骂。”孩子呆呆地望着菲拉，他简直不敢相信自己的耳朵。“那么，你知道前两位分别是谁吗？”菲拉停顿了一下，接着说，“第一个人叫富兰克林-罗斯福，是美国历史上唯一连任四届的总统。第二个人名叫丘吉尔，是英国历史上最著名的首相。”“孩子，”菲拉用手抚摸着孩子的头说，“你的人生才刚刚开始，过去的荣辱只能代表过去，让我们重新开始，好吗？”孩子双眼含着泪水，坚定地点了点头。
      许多年后，这个孩子的名字已被许多人知道，他的名字叫罗伯特-哈里森，他是华尔街上最年轻的基金经理人。
      
【温馨提示】

      你对孩子怎样描述，他们就怎样以你描述的样子成长。许多成人不断在用自己的偏见扼杀孩子的美质，他们自己却一点儿都不知道。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "拿破仑和士兵",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一天，拿破仑去检阅他的部队。正走着，他忽然听到远处传来一阵紧急的呼救声。于是，拿破仑急忙向呼救的地方奔去。起到湖边，拿破仑看到一个士兵正在水里手忙脚乱地挣扎，离岸越来越远。岸上的几个士兵则惊惶失措地励志呼喊。拿破仑问身边的士兵：“他会游泳吗？”一个士兵答道：“他只能比划几下，现在已不行了。陛下，怎么办？”显然，大家都在为水里的士兵担心着。拿破仑马上从侍卫手里拿过一支步枪，并冲落水的士兵大声喝道：“你再不向岸边游来，我就开枪了！”话音刚落，拿破仑真的端起枪，朝那人的前方连开了两枪。落水者听到槌响后，在惊恐中急忙调转方向，“扑通、扑通”地朝拿破仑所站的湖边游来。不一会儿，他便游到了岸边。落水的士兵得救了，他转过身子，正打算痛骂向自己开枪的人，却发现持枪站者竟是皇帝。于是他吓得魂飞魄散，但他仍不解地问：“陛下，我不小心掉进湖里，就快要淹死了，您为什么还要朝我开枪？”拿破仑笑道：“不吓你一下，你还有能力游上岸吗？那你才会真的淹死呢！”士兵们恍然大悟。
      
【温馨提示】

      实现理想需要渴望。渴望是成功的萌发点，把渴望转化为行动才能使之变成现实。一个人想成功首先要升自己成功的渴望。如果你还属于一个喜欢说“我想要”、“我希望”、“我需要”、“我会争取的人,v那么从现在开始，就学会说“我一定要”、“我肯定要”、“我绝对要”、“我必须要”、“我会全力以赴”！",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "心态决定命运",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      他父亲是印第安那州的农民，父亲去世时他才5岁。
      他14岁时从格林伍德学校辍学，开始了流浪生涯。
      他在农场干过杂活，干得很不开心而辞职。
      当过电车售票员，也很不开心而辞职。
      他16岁时谎报年龄参军，而军旅生活也不顺心。
      一年的服役期满后，他去了阿拉巴马州，自己当老板——开了个铁匠铺，不久就倒闭了。
      随后他在南方铁路公司当上了机车司炉工。他很喜欢这份工作，以为终于找到了自己的位置。
      他18岁时娶了媳妇，没想到仅过几个月时间，在得知太太怀孕的同一天又被解雇了。
      接着有一天，当他在外面忙着找工作时，太太卖了他们所有的财产逃回了娘家。不过，母女俩后来回到了他的身边。
      有一次还是在铁路上工作的时候，他曾通过函授学习法律，但后来放弃了。
      随后大萧条开始了，卖保险，不行；卖轮胎，赔本；经营渡船，出事；开加油站，失败。
      他成了考宾一家餐馆的主厨和洗瓶师。要不是那条新的公路刚好穿过那家餐馆，他会干得很好。接着到了退休的年龄。
      时光飞逝，眼看一辈子都过去了。而他却一无所有，要不是有一天邮递员给他送来了他的第一份社会保险支票，他还不会意识到自己老了。那天，哈伦德身上的什么东西愤怒了、觉醒了、爆发了。政府很同情他。政府说，轮到你击球时你都没打中，不用再打了。该是放弃、退休的时候了。他们寄给他一张退休金支票，说他“老”了。他愤怒了说：“凭什么？我的一生不过才刚刚开始！”他收下了那105美元的支票，并用它开创了新的事业——肯德基家乡鸡的成本。
      今天，他的事业欣欣向荣。而他，也终于在88岁高龄大获成功。
      这个到该结束时才开始的人就是哈伦德?山德士。他用他第一笔社会保险金创办的崭新事业正是肯德基家乡鸡。
      
【温馨提示】

      大仲马有句名言：“烦恼与欢喜，成功与失败，都只在一念之间。”抱有积极心态的人看到半杯水会说：“太棒了，还有半杯水！”消极的人则抱怨：“真糟糕，只有半杯水”。积极的人心满意足地从杯子里取水，他们活得轻松快乐；消极的人则处心积虑地往杯子里倒水，他们总是不快乐，而且活得很累，不能享受人生旅程的美好风光。改变态度就能改变生活，你怎么看这个世界，这个世界就是什么样子的。人们常说“心随境转”，其实，只要你愿意，完全可以做到“境随心转”。我们不能决定明天的天气，但是我们可以决定自己的心情；我们无法改变现实的，却可以改变对现实的看法。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "你也在井里吗",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一天，农夫的一头驴子,不小心掉进一口枯井里,农夫绞尽脑汁想办法救出驴子,但几个小时过去了,驴子还在井里痛苦地哀嚎着。最后,这位农夫决定放弃,他想这头驴子年纪大了,不值得大费周章去把它救出来,不过无论如何,这口井还是得填起来。于是农夫便请来左邻右舍帮忙一起将井中的驴子埋了,以免除它的痛苦.农夫的邻居们人手一把铲子,开始将泥土铲进枯井中。当这头驴子了解到自己的处境时,刚开始哭得很凄惨。但出人意料的是,一会儿之后这头驴子就安静下来了。农夫好奇地探头往井底一看,出现在眼前的景象令他大吃一惊:当铲进井里的泥土落在驴子的背部时,驴子的反应令人称奇——它将泥土抖落在一旁,然后站到铲进的泥土堆上面!就这样,驴子将大家铲倒在它身上的泥土全数抖落在井底,然后再站上去。很快地,这只驴子便得意地上升到井口,然后在众人惊讶的表情中快步地跑开了!
      
【温馨提示】

      就如驴子的情况,在生命的旅程中,有时候我们难免会陷入'枯井'里,会被各式各样的'泥沙'倾倒在我们身上,而想要从这些'枯井'脱困的秘诀就是:将'泥沙'抖落掉,然后站到上面去!人生必须渡过逆流才能走向更高的层次,最重要的是永远看得起自己.",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "大海里的船",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      英国劳埃德保险公司曾从拍卖市场买下一艘船，这艘船1894年下水,在大西洋上曾138次遭遇冰山,116次触礁,13次起火,207次被风暴扭断桅杆,然而它从没有沉没过。劳埃德保险公司基于它不可思议的经历及在保费方面给带来的可观收益，最后决定把它从荷兰买回来捐给国家.现在这艘船就停泊在英国萨伦港的国家船舶博物馆里。不过,使这艘船名扬天下的却是一名来此观光的律师。当时,他刚打输了一场官司,委托人也于不久前自杀了.尽管这不是他的第一次失败辩护,也不是他遇到的第一例自杀事件,然而,每当遇到这样的事情,他总有一种负罪感.他不知该怎样安慰这些在生意场上遭受了不幸的人。当他在萨伦船舶博物馆看到这艘船时,忽然有一种想法,为什么不让他们来参观参观这艘船呢?于是,他就把这艘船的历史抄下来和这艘船的照片一起挂在他的律师事务所里,每当商界的委托人请他辩护,无论输赢,他都建议他们去看看这艘船。它使我们知道:在大海上航行的船没有不带伤的。
      
【温馨提示】

      虽然屡遭挫折,却能够坚强地百折不挠地挺住,这就是成功的秘密.",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "野狼磨牙",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一只野狼卧在草上勤奋地磨牙，狐狸看到了，就对它说：“天气这么好，大家在休息娱乐，你也加入我们队伍中吧！”野狼没有说话，继续磨牙，把它的牙齿磨得又尖又利。狐狸奇怪地问道：“森林这么静，猎人和猎狗已经回家了，老虎也不在近处徘徊，又没有任何危险，你何必那么用劲磨牙呢？”野狼停下来回答说：“我磨牙并不是为了娱乐，你想想，如果有一天我被猎人或老虎追逐，到那时，我想磨牙也来不及了，而平时我就把牙磨好，到那时就可以保护自己了。”
      
【温馨提示】

      做事应该未雨绸缪，居安思危。“书到用时方恨少”，平常若不充实学问，临时抱佛脚是来不及的；也有人抱怨没有机会，然而当机会来临时，再叹自己平时没有积蓄足够的学识与能力，以致不能胜任，也只好后悔莫及。记住“机会青睐有准备的人”。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "疯子和呆子",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个心理学教授到疯人院参观，了解疯子的生活状态。一天下来，觉得这些人疯疯癫癫，行事出人意料，可算大开眼界。想不到准备返回时，发现自己的一个车胎被人下掉了。“一定是哪个疯子干的！”教授这样愤愤地想道，动手拿备胎准备装上。事情严重了，下车胎的人居然将螺丝也都下掉。没有螺丝有备胎也上不去啊！教授一筹莫展。在他着急万分的时候，一个疯子蹦蹦跳跳地过来了，嘴里唱着不知名的小曲，他发现了困境中的教授，停下来问发生了什么事。教授懒得理他，但出于礼貌还是告诉了他。疯子哈哈大笑说：“我有办法！”他从另外三个轮胎上下了一个螺丝，这样就拿到三个螺丝将备胎装了上去。教授惊奇感激之余，大为好奇：“请问你是怎么想到这个办法的？”疯子嘻嘻哈哈地笑道：“我是疯子，可我不是呆子啊！”
      
【温馨提示】

      每个人都有他自己的价值，只是某些人在某些领域特别突出而已。做人呆呆，处事聪明，在中国尤其不失为一种上佳做人姿态。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "抱抱法官",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      李夏普洛是个已经退休的法官，他天性极富爱心。终其一生，他总是以爱为前提，因为他明了爱是最伟大的力量。因此他总是拥抱别人。他的大学同学给他取了“抱抱法官”的绰号。甚至车子的保险杠都写着：“别烦我！拥抱我！”
      大约6年前，他发明了所谓的“拥抱装备”。外面写着：“一颗心换一个拥抱。”里面则包含30个背后可贴的刺绣小红心。他常带着“拥抱装备”到人群中，借着给一个红心，换一个拥抱。
      李因此而声名大噪，于是有许多人邀请他到相关的会议演讲；他总是和人分享“无条件的爱”这种概念。一次，在洛杉矶的会议中，地方小报向他挑战：“拥抱参加会议的人，当然很容易，因为他们是自己选择参加的，但这在真实生活中是行不通的。”他们要求李是否能在洛杉矶街头拥抱路人。大批的电视工作人员，尾随李到街头进行探访。首先李向经过的妇女打招呼：“嗨！我是李夏普洛，大家叫我‘抱抱法官’。我是否可以用这些爱心和你换一个拥抱。”妇女欣然同意，地方新闻的评论员则觉得这太简单了。李看看四周，他看到一个交通女警，正在开罚单给一台BMW的车主。李从容不迫地走上前去，所有的摄影机紧紧跟在后面。接着他说：“你看起来像需要一个拥抱，我是‘抱抱法官’，可以免费奉送一个拥抱。”那女警接受了。那位电视时事评论员出了最后的难题：“看，那边来了一辆公共汽车。众所皆知，洛杉矶的公共汽车司机最难缠，爱发牢骚，脾气又坏。让我们看看你能从司机身上得到拥抱吗？”李接受了这项挑战。当公车停靠到路旁时，李跟车上的司机攀谈：“嗨！我是李法官，人家叫我‘抱抱法官’。开车是一项压力很大的工作哦！我今天想拥抱一些人，好让人能卸下重担，再继续工作。你需不需要一个拥抱呢？”那位六尺二、二百三十磅的公车司机离开座位，走下车子，高兴地说：“好啊！”李拥抱他，还给了他一颗红心，看着车子离开还直说再见。采访的工作人员，个个无言以对。最后，那位评论员不得不承认，他服输了。
      一天，李的朋友南西?詹斯顿来拜访他。她是个职业小丑，身着小丑服装，画上小丑的脸谱。她来邀请李带着“拥抱装备”，一起去残疾之家，探望那里的朋友。他们到达之后，便开始分发气球、帽子、红心，并且拥抱那里的病人。李心里觉得很难过，因为他从没拥抱过临终的病人、严重智障或四肢麻痹的人。刚开始很勉强，但过了一会儿，南西和李受医师和护士的鼓励之后，便觉得容易得多了。数小时之后，他们终于来到了最后一个病房。在那里，李看到他这辈子所见过情况最糟的34个病人，顿时他的情绪变得十分复杂。他们的任务是要将爱心分出去，点亮病人心中的灯火，于是李和南西便开始分送欢乐。此时整个房间挤满了被鼓舞的医护人员。他们的领口全贴着小红心，头上还戴着可爱的气球帽。最后李来到最后一个病人李奥?纳德面前。李奥穿着一件白色围兜，神情呆滞地流着口水。李看他流着口水时，对南西说：“我们跳过去别管他吧！”南西回答：“可是他也是我们的一个朋友啊！”接着她将滑稽的气球帽放在李奥头上。李则是贴了一张小红心在围兜上。他深呼吸一下，弯下腰抱一下李奥。突然间，李奥开始嘻嘻大笑，其它的病人也开始把房间弄得叮当作响。李回过头想问医护人员这是怎么一回事时，只见所有的医师、护士都喜极而泣。李只好问护士长发生什么事了。李永远不会忘记她的回答：“23年来，我们头一次看到李奥笑了。”
      
【温馨提示】

      让别人的生命有一点不同，有一点亮光是何等简单啊！",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "爱人之心人皆应有",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      这是发生在英国的一个真实的故事：有位孤独的老人，无儿无女，又体弱多病。他决定搬到养老院去。老人宣布出售他漂亮的住宅。购买者闻讯蜂拥而至。住宅底价8万英镑，但人们很快就将它炒到了10万英镑。价钱还在不断攀升。老人深陷在沙发里，满目忧郁，是的，要不是健康情形不行，他是不会卖掉这栋陪他度过大半生的住宅的。
      一个衣着朴素的青年来到老人眼前，弯下腰，低声说：“先生，我也好想买这栋住宅，可我只有1万英镑。可是，如果您把住宅卖给我，我保证会让您依旧生活在这里，和我一起喝茶，读报，散步，天天都快快乐乐的——相信我，我会用整颗心来照顾您！”老人颔首微笑，把住宅以1万英镑的价钱卖给了他。
      
【温馨提示】

      完成梦想，不一定非得要冷酷地厮杀和欺诈，有时，只要你拥有一颗爱人之心就可以了。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "一只鸭子的黄金法则",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一只鸭子在辽阔的平原上练习飞翔，它天天想象着像一只老鹰一样能够展翅飞翔在天空，可是它一次次地摔倒，仍旧没有飞起来。树上一只小鸟问它母亲：那不是一只鸭子吗，它为什么那么拼命地想飞呢？母亲告诉它的孩子：那就是一只鸭子，但它总认为自己是只老鹰。
      寓言讽刺得很恰当，它告诉我们不要作无谓的选择，那对改变命运是无济于事的。可是，如果寓言继续下去，或许我们就不这样认为了。
      主人准备把这批鸭子卖掉，所有的鸭子都被捉起来了，当主人再去捉那只试图飞翔的鸭子时却没有能力捉住它了。那只鸭子动作很敏捷，最终逃脱了厄运，成为一只自由的鸭子。
      所以，那只看似愚蠢的鸭子没有白白练习飞翔，那一阵子的练习起了作用，起码它能逃过一劫。
      
【温馨提示】

      如果真的有改变命运的黄金法则（还是拿那只鸭子来讲）：从品行来说，它一定是善良的；从毅力来说，它一定是坚强的；从目标来说，它一定是有梦想的。你努力了，哪怕达不到最初的梦想，你也会有意想不到的收获。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "你到底关注什么",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      过去同一座山上，有两块相同的石头，三年后发生截然不同的变化，一块石头受到很多人的敬仰和膜拜，而另一块石头却受到别人的唾骂。这块石头极不平衡地说道：”老兄呀，曾经在三年前，我们同为一座山上的石头，今天产生这么大的差距，我的心里特别痛苦。”另一块石头答道：“老兄，你还记得吗？曾经在三年前，来了一个雕刻家，你害怕割在身上一刀刀的痛，你告诉他只要把你简单雕刻一下就可以了，而我那时想像未来的模样，不在乎割在身上一刀刀的痛，所以产生了今天的不同。”
      两者的差别：一个是关注想要的，一个是关注惧怕的。
      
【温馨提示】

      不管你希望拥有财富、事业、快乐，还是期望别的什么东西，都要明确它的方向在哪里？为什么要得到它？我将以何种态度和行动去得到它？人生教育之父卡耐基说：“我们不要看远方模糊的事情，要着手身边清晰的事物。”",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "用上你所有的力量",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个小男孩在他的玩具沙箱里玩耍，沙箱里有他的一些玩具小汽车、敞篷货车、塑料水桶和一把亮闪闪的塑料铲子。在松软的沙堆上修筑公路和隧道时，他在沙箱的中部发现一块巨大的岩石。小家伙开始挖掘岩石周围的沙子，企图把它从泥沙中弄出去。他是个很小的小男孩，而岩石却相当巨大。手脚并用，似乎没有费太大的力气，岩石便被他边推带滚地弄到了沙箱的边缘。不过，这时他才发现，他无法把岩石向上滚动、翻过沙箱边墙。小男孩下定决心，手推、肩挤、左摇右晃，一次又一次地向岩石发起冲击，可是，每当他刚刚觉得取得了一些进展的时候，岩石便滑脱了，重新掉进沙箱。小男孩只得哼哼直叫，拼出吃奶的力气猛推猛挤。但是，他得到的唯一回报便是岩石再次滚落回来，砸伤了他的手指。最后，他伤心地哭了起来。这整个过程，男孩的父亲从起居室的窗户里看得一清二楚。当泪珠滚过孩子的脸庞时，父亲来到了跟前。父亲的话温和而坚定：“儿子，你为什么不用上所有的力量呢？”垂头丧气的小男孩抽泣道：“但是我已经用尽全力了，爸爸，我已经尽力了！我用尽了我所有的力量！”“不对，儿子，”父亲亲切地纠正道，“你并没有用尽你所有的力量。你没有请求我的帮助。”父亲弯下腰，抱起岩石，将岩石搬出了沙箱。
      
【温馨提示】

      互有短长，你解决不了的问题，对你的朋友或亲人而言或许就是轻而易举的，记住，他们也是你的资源和力量。要善于借力，个人的力量对自然、对社会而言，都是渺小的，因此，要完成一件个人之力所不能及之事，须善于借用外界、他人的力量，才能达到目的。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "选择决定你的生活",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有三个人要被关进监狱三年，监狱长给他们三个一人一个要求。美国人爱抽雪茄，要了三箱雪茄。法国人最浪漫，要一个美丽的女子相伴。而犹太人说，他要一部与外界沟通的电话。
      三年过后，第一个冲出来的是美国人，嘴里鼻孔里塞满了雪茄，大喊道：“给我火，给我火！”原来他忘了要火了。
      接着出来的是法国人。只见他手里抱着一个小孩子，美丽女子手里牵着一个小孩子，肚子里还怀着第三个。
      最后出来的是犹太人，他紧紧握住监狱长的手说：“这三年来我每天与外界联系，我的生意不但没有停顿，反而增长了200％，为了表示感谢，我送你一辆劳施莱斯！”
      
【温馨提示】

      什么样的选择决定什么样的生活。今天的生活是由三年前我们的选择决定的，而今天我们的抉择将决定我们三年后的生活。我们要选择接触最新的信息，了解最新的趋势，从而更好的创造自己的将来。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "另类思维决定另类成功",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      100多年前，美国加州因发现金矿而吸引了大批淘金者加盟，犹太人莱维.施特劳斯也是这批淘金者之一，却每天以失望告终。一天，莱维和一位疲惫不堪的矿工坐在一起休息，这位矿工抱怨说：“唉，我们一整天拼命地挖啊挖，裤子破了也顾不上补。这鬼地方裤子破得特别快。”莱维眼前一亮，帆布不正是耐磨的布料吗？不久，第一条牛仔裤的前身工装裤就这样诞生了，并从加州迅速推向全国乃至全世界，莱维也由当初的贫困淘金者一跃而变成“牛仔裤大王”。
      
【温馨提示】

      成功就是这样简单，换个想法，避开竞争焦点的锋芒，迅速抢占潜藏的市场就可以了。但是，说起来简单做起来难，并不是每个人都能及时调整思维，准确地判断出潜在机遇的。人人渴望成功，但是成功却并不能被每个人分享。取得成功最简单的办法就是避开竞争激烈的领域，积极抢占只有七成胜算的“第二落点”。足球场中就有抢“第二落点”之说。在一般情况下，“第一落点”是有九成胜算的进攻位置，只要得手，极易进球，但是，也由于对方球员防守严密，进攻者总是无功而返；而“第二落点”由于少人跟防，往往会轻易取得战绩。足球场如此，追求成功的路上也是如此。一个人的潜力是无穷的，一旦定下心来，本身爆发的潜能自己也会吃惊，而死钻牛角尖只会将自己推进死胡同。重新调整成功的目标，尽管是痛苦的，但走出了第一步，再走第二步就顺畅多了。改变思维后，我们会发现成功原来如此简单。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "犹太人的学习智慧",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1、在犹太传统中，孩子第一次进学校总是先将用蜂蜜写在干净石板上的希伯来字母舔干净，希望他们知道学习是甜蜜而充满诱惑的。他们有一条万古不变的真理：财富可以被带走，唯有知识和智慧永不流失。
      2、敏锐的知性，可以抓住瞬间的机会，预见未来的趋势，洞悉细微外的微妙变化，把握宏观而抽象无形的东西。学习的目的便是培养这种洞若观火的洞察力。
      3、实际上，学习应该是怀疑、思考、提高知性能力的过程。知性并不是指知识。知性是以知识为基础，以完善知识、提高心性和能力为目的而构建起来的精神大厦。
      知识本身就是一笔财富，但犹太人看重的是将知识如何化为实实在在的物质财富，即人的知性，也就是人的智慧．因为智慧是打开幸福和财富之门的金钥匙。
      4、在犹太人眼中，学习不只是学习，而是以本身所学为基础,自行创造出新的东西的过程；学习的目的不在于培养另一个教师，也不是人的拷贝，而是在于创造一个新的人．世界之所以进步即在此。
      5、犹太人也蔑视一般的学习，他们认为一般的学习只是一味模仿，而不是任何创新．实际上，学习应该是怀疑，思考，提高知性能力的过程．只要是活着，犹太人总是不停地学习．因为对犹太人来说，学习是一种神圣的使命．犹太人认为，肯学习的人比知识丰富的人更伟大。
      6、在犹太教中，勤奋好学不只是仅次于敬神的一种美德，而且是敬神本身的一个组成部分．这种宗教般虔诚的求知精神在商业文化中的渗透，内化为犹太人孜孜不倦，探索求实，锐意进取的创新意识．他们孜孜以求在知识海洋中积累的丰富知识，对形成犹太人所特有的计划谋略与智慧发挥了文化滋养的作用．可以想象一个目不识丁的人或知识缺乏者在世界舞台上根本不会有运筹帷幄的智慧。
      
【温馨提示】

      成功就是这样简单，换个想法，避开竞争焦点的锋芒，迅速抢占潜藏的市场就可以了。但是，说起来简单做起来难，并不是每个人都能及时调整思维，准确地判断出潜在机遇的。人人渴望成功，但是成功却并不能被每个人分享。取得成功最简单的办法就是避开竞争激烈的领域，积极抢占只有七成胜算的“第二落点”。足球场中就有抢“第二落点”之说。在一般情况下，“第一落点”是有九成胜算的进攻位置，只要得手，极易进球，但是，也由于对方球员防守严密，进攻者总是无功而返；而“第二落点”由于少人跟防，往往会轻易取得战绩。足球场如此，追求成功的路上也是如此。一个人的潜力是无穷的，一旦定下心来，本身爆发的潜能自己也会吃惊，而死钻牛角尖只会将自己推进死胡同。重新调整成功的目标，尽管是痛苦的，但走出了第一步，再走第二步就顺畅多了。改变思维后，我们会发现成功原来如此简单。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "木匠和树",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个木匠前往一个城镇，在路上他看见一座土地庙的旁边有一棵高大无比的栎树。这是一棵巨大无比的栎树，它的树阴可供几千头牛在树下乘凉；树干又粗又直，在几丈高之后才能见到分枝，而这些树枝粗到可以用来做造船材料的就有好几十枝。许多路人都在围观，连声称奇，只有这个木匠瞄了一眼，扭头就走。同路人好奇地问道：“这是一棵多么高大华美的树木，你怎么看都不看一眼就走了呢？”木匠平静地说：“这棵树没什么用。用来造船，船会沉；做棺材，棺材会腐烂；做器具，器具会破裂；做门窗，门窗会流出汁液；做柱子，柱子会被虫蛀。正是因为它没有用，才会长这么长寿，这么高。”很奇怪，晚上，木匠竟然梦见这棵大树对他说：“你怎么能说我没用呢？我想想看，那些所谓的橘树、梨树和柚树，要果实成熟时，就会被人拉扯攀折，很快就会死掉。一切有用的东西无不如此。你眼中的无用，对我来说，正是大用。假如我像你所说的那样有用，岂不早就被砍了吗？”木匠醒来，若有所悟。他把这个梦和昨天的事告诉了妻子。妻子问道：“它既然向往无用，为什么要长在土地庙旁边，引人注意呢？”木匠答道：“如果它不是长在庙旁边，而是长在路中央，不也早被人砍掉当柴烧了吗？”
      
【温馨提示】

      世间没有绝对的标准。人们通常所谓的标准，都是人为制定的，它会随着时间以及社会的发展有所改变。千万不要用一把绝对的尺子作为衡量的标准。敢于质疑，敢于突破，才能取得更大的进步。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "两个用绳子结成的疙瘩",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个鲁国人用绳子结成两个疙瘩，这两个疙瘩很结实，常人一般是解不开的。他把这两个疙瘩送给宋元君，并说希望能有解开疙瘩的人。宋元君也希望自己的子民能接受这个挑战，把疙瘩解开，于是他向全国下令说：“凡是聪明的人，有技巧的人，都来解这两个疙瘩。”
      宋元君的命令引来了国内的能工巧匠和许多脑瓜子灵活的人。他们纷纷来解这两个疙瘩，可是却没有一个能够解开。他们只好摇摇头，无可奈何地离去。有一个叫倪说的人，不但学识丰富、智慧非凡，就连他的弟子，也很了不起。他的一个弟子对老师说：“让我前去一试，行吗？”倪说信任地点点头，示意他去。
      这个弟子拜见宋元君，宋元君叫左右拿出绳疙瘩让他解。只见他将两眼前打量一番，拿起其中一个，双手飞快地翻动，终于将疙瘩解开了。周围观看的人发出一片叫好声，宋元君也十分欣赏他的聪明能干。第二个疙瘩还摆在案上没动静。宋元君示意倪说的这个弟子继续解第二个疙瘩。可是这个弟子十分肯定地说：“不是我不解解开这个疙瘩，而且这疙瘩本来就是一个解不开的死结。”
      宋元君将信将疑，于是派人找来了那个鲁国人，把倪说弟子的答案说给他听。那个鲁国人听了，十分惊讶地说：“真了不起！的确是这样的，摆在案上的这个疙瘩是个没解的疙瘩。这是我亲手编制出来的，它没法解开，这一点，只有我知道，而倪说的弟子没有亲眼见我编制这个疙瘩，却能看出它是一个无法解开的死结，说明他的智慧是远远超过我的。”
      
【温馨提示】

      我们遇到难题时，一定要注意区别不同困难的性质，注意从实际出发，量力而行，千万不能死钻牛角尖，使自己越陷越深，以致不能自拔。",
            group5));

            group5.Items.Add(new SampleDataItem("Group-4-Item-3",
            "会登陆的鱼雷",
            "Item Subtitle: 3",
            "Assets/title/5.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在第二次世界大战中，苏联一艘潜艇发现德军在新罗西斯克港有个特殊的布防。在高厚的防波堤后面，修筑了迫击炮和大口径机枪阵地。苏军要在这个港口登陆，一定会遭到迫击炮和大口径机枪的猛烈反击。舰上的炮火打不到它，用飞机去轰炸，但敌人防空力量很强，因此，很难一举歼灭迫击炮阵地。苏军得到这个情报，多次召开作战会议，重点研究如何打掉这上迫击炮阵地。有位舰长提出：借用鱼雷对付迫击炮。人们一听都哈哈大笑，觉得这太荒唐了，根本不可能实现。
      这位舰长坚持了自己的意见，他认为借用鱼雷打迫击炮阵地完全有可能的，为能让苏军采纳自己的意见，他说了亲眼见到的事：在一次演习中，他的一枚鱼雷从海面冲到沙滩上，并向前滑行了20米，说明鱼雷能登陆作战。这一下引起指挥员的深浓兴趣，在别无他法的情况下，苏军下令成立专门小组研究“鱼雷登陆作战”的难题。当时最大的难题是如何防止鱼雷碰撞防波堤后爆炸，并使它超过防波堤在着陆后最远点上爆炸，不然很难消灭防波堤后的迫击炮兵阵地。为此，他们制造了一种合适的惯性引信，使鱼雷飞过防波堤高度之后爆炸。改装之后经过实弹射击，鱼雷登陆成功。
      攻击新罗西斯克港的战斗打响后，苏军一个中队的鱼雷艇朝港内防波堤发射10枚鱼雷。这些鱼雷冲出水面越过防波堤之后爆炸，把德军迫击炮阵地、大口径机关枪阵地炸得稀巴烂，使其瞬间失去战斗力，苏军发起登陆，很快占领港口。事后，几个活着的德国炮兵，纳闷地问苏军士兵：“那些从水里上来，会翻腾的炸弹，到底是什么武器？”当苏军士兵告诉他们是鱼雷时，这几个德国炮兵断地摇头：“没听说过还有会登陆的鱼雷！”
      舰长的意见可谓是匠心独运的高深智慧。他突破习惯思维，并坚持自己的意见。
      
【温馨提示】

      如果我们在质疑时遭到了别人的嘲笑，应该坚持到底，让事实来证明我们的正确。",
            group5));



            this.AllGroups.Add(group5);
            #endregion

            #region 第6章
            var group6 = new SampleDataGroup("Group-6",
            "第6章",
            "Group Subtitle: 1",
            "Assets/title/6.png",
            "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
"狼王的胜利",
"Item Subtitle: 3",
"Assets/title/6.png",
"Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
@"
      那是一个极度干旱的季度，因为缺少水和食物非洲草原上的许多动物都死去了。生活在这里的鬣狗和狼也面临同样的问题。一般狼群外出捕猎统一由狼王指挥，而鬣狗却是一窝蜂地往前冲，鬣狗仗着数量众多，常常从猎豹和狮子的嘴里抢夺食物。由于八和鬣狗都属犬科动物，所以能够相处在同一片区域，甚至共同捕猎。可是在食物短缺的季节里，狼和鬣狗也会发生冲突。一次，它们就因为争夺一头野牛的残骸，一群狼和一群鬣狗发生了冲突，场面非常的惨烈。鬣狗死伤惨重，但由于数量比狼多得多，很多狼也被鬣狗咬死了，最后，只剩下一只狼王与5只鬣狗对峙。
      狼王与鬣狗力量悬殊，何况狼王还是在混战中被咬伤了一条后腿。那条拖拉在地上的后腿，是狼王无法摆脱的负担。面对步步紧逼的鬣狗，狼王突然回头一口咬断了自己的伤腿，然后向离自己最近的那只鬣狗猛扑过去，以迅雷不及掩耳之势咬断了它的喉咙。其它4只鬣狗被狼王的举动吓呆了，都站在原地不敢向前。终于，4只鬣狗拖着疲惫的身体一步一步地离开了怒目而视的狼王。狼王胜利了。
      
【温馨提示】

      生活中，有很多东西拖我们的后腿，使我们瞻前顾后，患得患失，不能集中精力解决问题。有魄力的人往往会果断地舍弃这些东西。如果不懂得放弃，我们做事就不会有效率，就无法获得更大的成功。",
group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "狐狸与山羊",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      森林里有一只狐狸失足掉到了井里，那口井很深，不论它如何努力，如何挣扎仍没法爬上去，还好井下有一块石头，狐狸站在上面，才不被淹死，但水已经漫到半身了。狐狸只能望着井口发呆。碰巧一只公山羊觉得口渴极了，来到井边，看见狐狸在井里，便问他井水好不好喝。狐狸心中暗喜，觉得机会来了，马上镇静焉，向公山羊极力赞美井水好喝：“山羊，这水是天下第一泉，清甜爽口，喝了倍感精神，你赶快下来喝个痛快吧。”公山羊一心只想喝水，便对狐狸的话信以为真，不假思索地跳了下去。当公山羊“咕咚咕咚”痛饮完后，才发现自己已经上不去了，就不得不与狐狸一起共商出井的方法。狐狸心里早已打好了如意算盘，他狡猾地说：“我倒有一个方法：你用前脚扒在井壁上，再把角竖直了，我从你后背跳上井去，再拉你上来，我们就都得救了。”公山羊觉得狐狸的这个办法很好，便同意了他的提议。公山羊按照狐狸说的办法把狐狸送出了井。狐狸上去以后，却准备独自走了。公山羊十分生气，指责狐狸不信守诺言。狐狸回过头对公山羊说：“喂，朋友，你的头脑如果像你的胡须那样完美，你就不至于没看清出口之前就盲目地跳下去了。”
      
【温馨提示】

      一位哲人曾这样告诫他的学生：“在进去之前，一定要确保自己能够顺利地出来。”采取一项行动之前，一定要考虑清楚可能的后果，不要盲目地去干，否则就会像故事中的公山羊，出不了井。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "橙子",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      西班牙曾有位叫彼得罗一世的国王，对于很多人来说，他是正义的象征。
      一天，彼得罗一世宣布凶将公开选拔法官，消息一传出去，很多人都跃跃欲试。
      这天，有三个人来毛遂自荐，其中有一个是宫廷的贵族，一个是曾经陪伴国王南征北战的勇敢的武士，还有一个是普通的教师。国王决定对他们进行一次特殊的考试。
      于是，在宫廷人员和三位候选人的陪伴下，国王离开王宫，率领众人来到池塘边，只见池塘上漂浮着几个橙子。众人们都不明白国王的意思。
      “池塘上一共漂着几个橙子啊？”这时国王发话了。贵族走到池塘边，开始点数。
      “一共是6个，陛下。”
      国王没有表态，继续问武士同样的问题：“池塘上一共漂着几个橙子啊？”
      “我也看到了6个，陛下！”武士甚至没有走近池塘就直接回答了国王的问题。
      国王没有说话。
      “池塘里有多少个橙子啊？”他最后问教师。
      教师什么也没有说，径直走近池塘，脱掉鞋子，进到水里，把橙子拿出来。
      “陛下，一共有3个橙子！因为它们都被从中间切开了。”教师说。
      “只有你知道如何执法。”国王说，“在得出最后的结论之前，我们应该加以证明，因为并不是所有我们看到的都是事情的真相。”
      
【温馨提示】

      教师是聪明谨慎的，他通过亲自实践去求证答案。所以在判断某件事情时，不要人云亦云，更不要妄下结论。学习也一样，有时老师在黑板上列一道题，有一个同学说出了答案，其他的同学也跟着喊出同样的答案，有时这个答案未必是正确的。很多时候，我们所看到的听到的并不是事情的真相。只有自己亲自去实践，才能知道事情的真相究竟是什么，才能真正掌握好知识。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "回避尴尬",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      富兰克林深受世人的敬仰，不仅因为他是美国的开国元勋和杰出的科学家、政治家，更因为他一直被后人推崇为人类精神最完美的典范。一天，富兰克林和助手一道外出办事。他们来到办公楼的出口处时，看见前面不远处正走着一位妙龄女郎。由于她走得太匆忙，突然脚下一个趔趄，身体失去平衡，一下子就跌坐在地上。富兰克林一眼就认出了她，她是一位平时很注重自己外在形象的职员，总是修饰得大方得体，光彩照人。助手见状，刚要上前去扶她，却被富兰克林一把拉住，并示意他暂时回避。于是，两人很快折回到走廊的拐角处，悄悄地关注着那个女职员的一举一动。助手不明白富兰克林的意思，看他满脸困惑的神情，富兰克林只轻轻地告诉他：“不是不要帮她，而是现在还不是时候，再等等看吧。”一会儿，那位女职员就站起来，环顾四周，掸去身上的尘土，很快恢复了常态，若无其事地继续前行。等那个女职员渐行渐远，助手仍有些不解。富兰克林淡淡一笑，反问道：“年轻人，你难道就愿意让人看到自己摔跤时那副倒霉的样子吗？”助手听后，顿时恍然大悟。
      
【温馨提示】

      做什么事都要相时而动，关于把握时机的人，能扭转时局，把坏事做成好事；而那些不分时机场合，做事冒冒失失的人，即便是出于好心，往往也不会达到预期的效果，有时甚至还会把事情办糟。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "只看到了兔子",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一天，父亲带着三个孩子到森林里打猎，他们的目标是兔子。
      来到森林里，父亲问老大看到了什么。老大回答说：“我看到了猎枪、兔子，还有大树。”父亲摇摇头说：“不对”。
      父亲以相同的问题问老二。老二回答：“我看到了爸爸、大哥、弟弟，以及猎枪、兔子。”父亲又摇摇头说：“不对。”
      父亲再问老三。老三回答：“我只看到了兔子。”父亲高兴地点点头说：“这就对了。”
      
【温馨提示】

      无论做任何事情，三心二意都是最大的障碍，不把全部精力集中在你要做的事情上，而去想其它无关的东西，心猿意马，就难免会分散精力，更不用说提高办事效率了。一个人的精力是有限的，没有足够的精力投入到学习上，那么学习也不会有很大的长进。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "像水一样学会绕道而行",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      谢里曼在幼年时读过一本叫《荷马史诗》的书，他深深被书里所记载的故事吸引了，暗下决心，一旦他有了足够的收入，就投身考古研究。谢里曼很清楚，进行考古发掘和研究是需要很多钱的，而自己家境十分贫寒，现实与理想之间存在着很大的差距，他没有直线可走，只能走曲线。
      于是，从12岁起，谢里曼就自己挣钱谋生，先后做过学徒、售货员、见习水手、银行信差等等。慢慢的谢里曼好起来了，他后来在俄罗斯开了一家商务办事处。谢里曼有了自己的事业后，从未忘记过自己的理想。利用业余时间，他自修古代希腊语，而通过参与各国之间的商务活动，他学会了多门外语，这些都为日后的成功打下了基础。
      多年以后，谢里曼终于在经营俄国石油业中积攒了一大笔钱。当人们以为他会大大享受一番时，他却放弃了有利可图的商业，把全部的时间和钱财都花在追求儿时的理想上了。
      谢里曼坚信，通过发掘，一定能够找到《伊利亚特》和《奥德赛》中所描述的城市、古战场遗址和那些英雄的坟墓。1870年，他开始在特洛伊挖掘。不出几年，他就挖掘出了9座城市，并最终挖到了两座爱琴海古城：迈锡尼和梯林斯。这样，谢里曼就成了发现高度发达的爱琴海文明的第一人，这一发现在世界文明史中有着重要意义。
      
【温馨提示】

      “条条道路通罗马”，当遇到某些问题我们无法直接解决时，就要学地去尝试不同的路径。直线距离虽短，但如果前面是座大山，我们硬要冲过去，肯定会撞得头破血流。所以，有时候我们应该像水一样，学会绕道而行。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "一双放风筝的手",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      毕业的那年，从小到大不曾离开过家的我，为了自己的理想，放下年老的爸妈，一个人到花莲工作。家中的三个姐姐都已嫁做人妇，爸妈对我这个唯一的幺女，其实一直非常的放心不下。
      刚到花莲的第一个月，我的工作尚未进入状况、人生地不熟，自己唯一的感觉就是想家。记得我第一次从花莲回家时，来去匆匆。星期六的晚上近十一点才到家，想不到平日早睡的爸妈站在阳台上等我回家，在我走进他们视线前，他们不知已站了多久，当我走进巷口抬头看见爸爸时，忽然觉得出门在外的委屈都一起涌上了心头。
      第二天的下午要离开家去搭车时，不善表达的爸妈在送我出门前告诉我：“如果不习惯就回家，不要委屈了自己。”走下了楼，抬头又见爸妈在窗口孤单的身影，及对我的不舍，让我几乎无法走出巷口。后来，工作逐渐的步上轨道，时常有机会全国来回出差，却没有回家的时间，每次经过台北，想到家中那对等待的父母，心中有种说不出的遗憾。
      有一次，我到台中参加一个会议。结束会议时，已是晚上九点多了，原本的安排是住宿在台中，第二天搭飞机返回花莲。可是我想到自己已好久没有回家，也许可以赶上九点多的火车回台北过一夜，看看爸妈。于是赶到火车站买票，拨了电话告诉爸爸别等门，我会在今晚搭车回家。那一天的火车不知为什么特别的拥挤，误点了二十多分钟，到达台北已是十二点半了。我低头匆匆的赶路；就在我走到车站门口的时候，有一只手搭在我的肩头，回头一看，竟是我满头白发的老爸。我难掩心中的感动，却仍是说不出感谢的话，只好责怪爸爸：“不是告诉你，不要等我，你又不知道我几点会到，这么晚了，你几点来的？”看着爸爸低头不说话，我只有满心的歉疚。爸爸接到我的电话就出门，一个人在车站等了近三个小时，我怎能不怪他傻？
      第二天当我一个人坐在返回花莲的火车上，回想起昨夜爸爸的白发时，眼泪就忍不住落了下来，其实这不是爸爸第一次等我。以前也有许多次的晚归，让爸爸等门，而不懂事的我，总是满心的不耐。但是这一次，我一个人在外生活了一段时间，在我走出车站看见爸爸的那一刻，才发现自己有多幸福，才能体会出爸妈对子女的牵挂。
      想起了一段话，已不记得是在那一本书上看到的：“父母的手就像一双放风筝的手，风筝飞不起来时，他们尽全力的想让风筝飞在天空；风筝飞起来了，又担心风筝飞得太高太远而断了线，每一对父母就像放风筝的手忙碌于放线、收线之中，而忘了自己。”
      
【温馨提示】

      “条条道路通罗马”，当遇到某些问题我们无法直接解决时，就要学地去尝试不同的路径。直线距离虽短，但如果前面是座大山，我们硬要冲过去，肯定会撞得头破血流。所以，有时候我们应该像水一样，学会绕道而行。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "压力就是甜点，只要你能逆向观看",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      记住anger（发怒）与danger（危险）只差一个字。别少看这一个字，它会影响我们这份职业、甚至人际关系，正所谓“祸从口出…”。
      专家讲：“人，面对外人时，总是可以表现得雍容大肚、心平气和，但面对自己最亲近的家人，却往往一点小事就足以皱起眉头，甚至出言相伤。”
      正如你永远只对你最爱、最亲的人生气，你会对住你上司、老板生气吗？如果形容人类是一种“出门高E.Q，回家低E.Q”的动物，我想一点儿也不夸张。多拿出一点耐心与包容给家人吧！否则您自己也不会快乐，不是吗？
      曾在报上看过，英文中的stressed（压力），与desserts（甜点）两字，有很微妙的相关。是什么相关呢？仔细一瞧，好像没什么关系嘛！可是，再看一下，咦，stressed这个字从后面倒过来拼写，不就是desserts吗？所以，“Stressedisjustdessertsifyoucanreverse.”（压力就是甜点，只要你能逆向观看。）哈，这真是有趣的妙论呀！
      人生之中，有许多「压力、挫折」，但只要转个念、换个角度看，它就是我们生命中的「甜点」呀！此外，也有人说：「人生就像一碗饭，一半是甜的，一半是苦的，你不知道会先吃到哪一边，但终究必须把饭吃完。」是呀，生命有甜、有苦、有酸、也有辣；
      但都必须去经历它、走过它呀！
      有一个小学老师在偏远的乡里教书，这天，他来到自己班上的教室，问班上的小朋友：「你们大家有没有讨厌的人啊」小朋友们想了想，有的未做声，有的则猛力地点点头。老师接着便发给每人一个袋子，说：「我们来玩一个游戏。现在大家想想看，过去这一周，曾有那些人得罪过你他到底做了怎么样可恶的事想到后，就利用放学时间到河边去找一块石头，把他的名字给用小纸条贴在石头上，如果他实在很过分，你就找一块大一点的石头，如果他的错是小错，你就找一块小一点的石头。每天把战利品用袋子装到学校来给老师看哦！」
      学生们感到非常有趣且新鲜，放学后，每个人都抢着到河边去找石头。第二天一早，大家都把装着从河边捡来的鹅卵石的袋子带到学校来，兴高采烈地讨论着。一天过去了，两天过去了，三天过去了….，有的人的袋子越装越大，几乎成了负担。
      终于，有人提出了抗议“老师，好累喔！”老师笑了笑没说话，立刻又有人接着喊：“对啊每天背着这些石头来上课，好累喔！”这时，老师终于开口了，她笑着说：“那就放下这些代表着别人过犯的石头吧！”孩子们有些讶异，老师又接着讲：“学习宽恕别人的过犯，不要把它当宝一样的记在心上，扛在肩上，时间久了，任谁也受不了……”
      这个星期，这班的同学上到了人生中极宝贵的一课。袋里装入越多、越大的“石头”，心中留存越多、越深的仇恨，所造成的负担就越重。假如你有写上我名字的石头，你知道怎样做……
      感谢主我很喜欢一句名谚：“宽恕人的过失，便是自己的荣耀。”懂得“放下”，何等自在。
      相爱容易相处难，婚姻与恋爱最大的不同，就是“恋爱看的是对方的优点”，而“婚姻却是要包容对方的缺点”，缘分是找到包容你的人！
      
【温馨提示】

      “条条道路通罗马”，当遇到某些问题我们无法直接解决时，就要学地去尝试不同的路径。直线距离虽短，但如果前面是座大山，我们硬要冲过去，肯定会撞得头破血流。所以，有时候我们应该像水一样，学会绕道而行。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "90/10 的定律",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      它将改变你的一生（至少它将改变你对不同情况的反应）。
      90/10的定律是什么？生命的10%是由你的际遇所组成，余下的90%则由你的反应而决定。
      这意味着什么？我们无法掌握那10%的际遇。我们无法阻止行程因汽车坏掉、航班误点，甚是有人在塞车时争车道挡住你的车子。我们无法控制那10%的际遇，但余下的90%则不然。你可以决定余下的90%。
      如何？…凭你的反应。你不能控制交通灯转红，但你能够控制你的反应。别让他人愚弄你，你能够控制你的反应。
      让我们举个例子。
      你与你的家人吃早餐，你的女儿不小心把咖啡泼倒在你的衬衫上，这是你无法控制的情况。
      下一步将如何则由你的反应而定。
      你开始责骂，你狠狠地臭骂女儿一顿，令她陷于痛苦之中。然后你又把怨气发泄在太太身上，责难她把咖啡放在桌边。接踵而来的是一场短暂的骂战。你生气地上楼更换你的衬衫。你下楼，然后发现你的女儿由于哭泣来不及吃早餐和准备上学。结果，她错过了巴士。
      你的太太赶着上班，你匆忙开车把女儿送回学校。因为你已经迟到了，你以时速四十英里在一条限速三十英里上的道路前进。
      你付了六十元道路罚款，终于抵达学校，并已迟到十五分钟。你的女儿没有跟你道别便已跑进学校。你回到公司，已是九时二十分了，这时你竟然发现──你忘了带公事包。这是非常糟糕的一天，而你感到你的运气每况愈下，你开始渴望回家。当你下班回家，你感到你与太太及女儿的关系上出现微小裂痕。
      为何你会有如此糟糕的一天？…一切皆由你早上的反应而起。
      Ａ）是咖啡所造成的吗？
      Ｂ）是你的女儿所造成的吗？
      Ｃ）是警察所造成的吗？
      Ｄ）是你所造成的吗？
      答案是Ｄ。
      你无法控制女儿打翻咖啡一事，但你在紧接那五秒内的反应让霉运开始发生。
      以下是你改写命运的结局。
      咖啡翻倒在你身上，你的女儿几乎要哭了，但你温柔地说：“亲爱的，这并不算什么，但你下次得小心一点了。”你拿起毛巾便上楼去。在你更衣完毕并拿起你的公事包后，你下楼去，望出窗外，你看到你的孩子正在上巴士。她回头并向你挥手。你早了五分钟回到公司，并亲切地与你的同事打招呼。你的上司亦对你新的一天给予正面的评价。
      看到两者的分别吗？两个不同的情景，由同一个开首所引起。但结局完全两样。
      Why?为什么？
      Becauseofhowyoureacted.皆因你的反应而起。
      你或许真的无法掌控10%的际遇，但剩下的90%则可以由你的反应而定。
      以下有一些实践90/10定律的方法。如有人说起你的是非，千万别当一块“海绵”，让那些攻击性的说话像水在玻璃上一般的流走。别让那些负面评价缠绕着你！适当的回应能够使你的生活免受破坏。一个错误的反应能够使你失去朋友、被解雇、甚或被压力压得喘不过气来。
      如果有人在塞车时争车道挡住你的车子，你会如何感想？你会生气吗？你会否猛击车上的方向盘？我的一个朋友就把方向盘弄坏了！你会怒骂吗？你的血压是否急速上升？你会否尝试去撞他们？
      谁会在意你上班迟到了十秒？为何让车子破坏你的驾驶过程？
      Rememberthe90/10principle,anddonotworryaboutit.记着90/10的定律，别在意。
      你被通知说你被解雇了。
      你为何失眠与愤怒?事情总是会解决。不如用你忧心的力量及时间去找寻新工作吧！
      航班延误了，而它将影响你一整天的行程。为何将你的怒气发泄在服务员的身上？她并不能阻止事情发生。
      如利用你的时间学习，或认识旁边的乘客。不要愤怒，它将令你的一天变的更糟。
      现在，你懂得了90/10的定律。实践它，你将会发现它的惊人效果。尝试实行它，你将不会有任何损失。90/10的定律非常神奇，而只有很少数的人懂得运用它。
      Theresult?结果呢？成千上万的人沉溺在痛苦、尝试、问题与伤心之中。我们必须理解并实践90/10的定律。
      Itcanchangeyourlife!!!Enjoy……它将改变你的一生。
      
【温馨提示】

      “条条道路通罗马”，当遇到某些问题我们无法直接解决时，就要学地去尝试不同的路径。直线距离虽短，但如果前面是座大山，我们硬要冲过去，肯定会撞得头破血流。所以，有时候我们应该像水一样，学会绕道而行。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "免费拥抱后记",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个短片名为《FreeHugs》，短短四分钟左右，却吸引了无数人目光。
      影片一开始是一个嬉皮式打扮的年轻人手上拿着一个牌子高高顶在头上，向路人展示。牌子上写的是FREEHUGS（照荳芽的翻译是：抱一抱！免钱！）。荳芽一看到他的牌子就哈哈大笑。影片里的年轻人这时可笑不出来，在悠扬的配乐声中，他独自一个人举着牌子在来来往往的人群中踱步，没人来跟他抱一下，有的人看他一眼、躲得远远的走开，有的人根本连看也不看他。后来一个矮小的老婆婆在他面前停住，高大的年轻人单膝跪下来，这才跟老婆婆一般高。一直有音乐，所以我们无法听见老婆婆跟他说什么，反正老婆婆就是跟他抱了！
      抱一抱，老婆婆笑了。还用手轻轻捏了一下年轻人的脸颊，年轻人也笑了。一旁一个年轻小姐见了，也上来抱抱了……之后，歌声渐渐变得愈来愈激昂。很多经过的路人一个个都来跟年轻人抱抱。大家抱的方式都不一样，有的摆出相扑选手的架势后、猛扑上去抱，有的先助跑后、狠狠的跳上去抱，还有一个孩子先从远处滑着滑板冲上来、凌空跃起、抱。每一个拥抱的人，都笑了。后来电视来采访了，记者们也抱一下。围观的群众，当然也抱。一直抱到影片结束。我再也记不起有多少人用多少不同的方式拥抱，只深深感受到──温暖的感动。
      这个叫JuanMann的年轻人是个英国人，他说有一阵子他诸事不顺，心情很沮丧。有一天，他从国外搭机回伦敦，在机场，他看到很多旅人下飞机就有亲人朋友来接机，他们拥抱、微笑、亲吻。只有他孑然一身。很孤单。
      然后，他觉得他很需要一个拥抱。于是，他做了一个牌子，上面写着“FreeHugs”，走到路上去。这就是我们后来看到的影片内容……
      第一个跟他抱抱的老婆婆原来那一天也很难过，她的小狗刚刚过世了。她看到年轻人的FreeHugs牌子，也觉得自己需要跟人抱抱、被安慰一下，便上前去跟Juan说她的难过，然后抱抱。
      这个“FreeHugs”现在已经变成一个在全世界进行的运动了。我看到韩国、美国、俄罗斯、巴西、菲律宾……很多国家的年轻人拍了自己的“FreeHugs”放上网去，配上不同的配乐，用自己的方式表现“FreeHugs”的友爱精神。一个悲伤的年轻人的无聊举动，竟演变成一股足以影响世界的力量。你能说这世界不神奇吗？
      我把这个片子介绍给荳芽看，跟她说：“可不要小看妳无聊时做的一件小好事呢！”荳芽拿起我的一本无字书，看图说故事……
      她说：“有一个小孩……妳看！他很无聊、呆呆看着窗户外面。他就去跟妈妈要钱、跑出去买面包。才刚刚咬一口，他看到一个瘦巴巴的老爷爷躺在公园的椅子上睡觉。老爷爷好可怜，没有家，还睡在公园。那个小孩子就把面包放在老爷爷旁边，回家去了。老爷爷醒了，好开心看到面包，因为他很饿了。他吃的时候，看到小鸟，就分一点点给小鸟吃，鸟爸爸鸟妈妈就把面包带一点回家去喂它们的小baby，鸟baby吃了面包就一直长大、长大……然后飞出去玩玩。鸟baby飞到公园，遇见老爷爷，老爷爷给牠一颗种子，鸟baby就飞走了，它飞到那个小孩家的窗户旁边，停下来休息，种子不小心掉下去了，后来下雨了，种子就一直长大、长大，变成很漂亮的一朵花。那个很无聊的小孩看到花，就说：‘哇！好漂亮！’就笑了……”
      这本书，我们给它一个名字，叫《一个神的善意》。它一个字都没有。你却隐约可以听到它用一种很温柔的、让人听了会想找一个人好好抱抱的声音，说故事……
      荳芽指着这书的英文原名问我：“为什么它叫ACIRCLEOFFRIENDS？”
      人生，是一个圆。你给出去的，会再回到你身上来。所以，我的回答是：“荳芽，妳想要什么，就先给别人什么……”
      
【温馨提示】

      “条条道路通罗马”，当遇到某些问题我们无法直接解决时，就要学地去尝试不同的路径。直线距离虽短，但如果前面是座大山，我们硬要冲过去，肯定会撞得头破血流。所以，有时候我们应该像水一样，学会绕道而行。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "比佛的吉尼斯纪录",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      比佛是英国吉尼斯啤酒厂的总经理，他喜欢在假期约朋友一起打猎。他对自己的枪法十分满意，经常在朋友们面前吹嘘，自己可以打到任何猎物。
      有一次，他们发现一种鸟飞得特别快，朋友们就和比佛打赌，看他能否射中这种鸟。结果比佛一只也没打中，朋友借此对他的枪法大加嘲弄。比佛认为这不是他的枪法不好，而是这种鸟飞得实在太快了。但朋友们却不这样认为．激烈的争执之下，比佛开始认真了。他认定那种鸟是世界上飞行最快的鸟。
      为了证明自己的说法是正确的，比佛在打猎回来之后，就找出了《百科知识》之类的书进行查阅，他想通过书上的记载让朋友心服口服。但比佛耗费了大量时间，却没有得到任何有价值的资料，没有一本书提及鸟儿飞行的速度问题。比佛很失望，他没有找到证据证明自己的说法是正确的。比佛灵感突发，他想，既然世界上没有一本书记载鸟儿飞行的速度，为什么自己不编一本这样的书呢？
      他通过朋友介绍，聘请了两位孪生兄弟担任编辑。一年后，他们编出第一本样书，比佛把它取名为《吉尼斯世界纪录大全》。这本书一上市就受到读者的欢迎。自它面世以来，平均每年出一版，被翻译成21种文字，发行量达到4000万册，成为世界上最畅销的书。
      
【温馨提示】

      碰到难题时你应该思考，这是一次改变的机遇。你领悟了，你就得到新生；你没有领悟，生活就会把这个机遇转交给另一个人。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "人生就是马拉松",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在1984年的东京国际马拉松邀请赛中，名不见经传的日本选手山田本一出人意料地夺取了世界冠军。当记者问他凭什么取得如此惊人的成绩时，他说了这样一句话：凭智慧战胜对手。当时许多人认为这个小个子选手是在故弄玄虚。马拉松是体力和耐力的运动，只要身体素质好又有耐性就有望夺冠，爆发力和速度都还在其次，说是用智慧取胜确实有点勉强。
      两年后，意大利马拉松邀请赛在北部城市米兰举行，山田本一代表日本参赛──他又获得了世界冠军。记者又请他谈经验。山田本一回答的仍是上次那句话：用智慧战胜对手。这回记者在报纸上没再挖苦他，但对他所谓的“智慧”仍迷惑不解。
      几年后，这个谜终于被解开了。他在自传中写道。“每次比赛前，我都要乘车把比赛线路仔细看一遍，并把沿途比较醒目的标志画下来。比如第一个标志是银行，第二个标志是一棵大树，第三个标志是一所红房子……这样一直画到赛程的终点。比赛开始后，我就以百米速度奋力向第一个目标冲去，等越过第一个目标后，我又以同样的速度向第二个目标冲去。40多公里的赛程，就被我分解成这么几个小目标轻松地跑完了。起初我并不懂这样的道理，我把我的目标定在40公里外终点线的那面旗帜上。结果跑到十几公里时，我就疲惫不堪，我被前面那段遥远的路程给吓倒了。”
      
【温馨提示】

      现实生活中半途而废的原因，往往不是因为难度较大，而是觉得成功离我们较远，确切地说，我们不是因为失败而放弃，而是因为倦怠而失败。在人生的旅途就是马拉松，设定一个正确的目标不容易，实现目标更难，把一个大目标科学地分解为若干个小目标，落实到每天中的每一件事上，不失为一种大智慧。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "谁为时间埋单",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      人们对于金钱的开支，大多比较留心，但对于时间的支出，却往往不大在意。如果有谁为人们在工作生活等方面所用去的时间一一予以记录，列出一份“生命的账单”，不仅十分有趣，而且可能会令人有所感悟，有所警醒。
      法国《兴趣》杂志对人一生在时间的支配上做过一次调查，结果是这样的：
      站着，30年；
      睡着，23年；
      坐着，17年；
      走着，16年；
      跑着，1年零75天；
      吃着，7年；
      看电视，6年；
      闲聊，5年零258天；
      开车，5年；
      生气，4年；
      做饭，3年零195天；
      穿衣，1年零166天；
      排队，1年零135天；
      过节，1年零75天；
      喝酒，2年；人厕，195天；
      刷牙，92天；
      哭，50天；
      说‘你好’，8天；
      看时间，3天......
      英国广播公司也曾委托人体研究专家对人的一生进行了“量化”分析，有些数字可以作为上面推算的补充：
      沐浴，2年；
      等候入睡，18周；
      打电话，2年半；
      等人回电话，14周；
      无所事事，2年半......
      以上推算和量化分析并不全面，而且有些数字也不具有很强的说服力和可信性，但为我们大致列出了一个生命的账单。这份账单上的时间开支，有一些是非花销不可的，但有的却完全可以节省。每个人在生活的每一天都必须清楚：我该为哪些事花费时间？哪一些可以忽略或缩短？只有像对金钱那样计较时间，我们才能在有限的人生中做更多有意义的事情。
      
【温馨提示】

      我们看不见时间，这就是我们的悲剧所在。因为看不见，我们不知道它到底有多少；因为看不见，它引不起我们的关注。时间就像我们手里的信用卡，如果你不小心使用，终会有一天它的余额突然变成了零──而你还有那么多事还没有做，还有那么多美妙的计划还未实施！而且，它不像信用卡──它不能充值。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "狐狸的遭遇",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有只狐狸惊慌失措地跑进一个村落，喘得上气不接下气，四肢发软，狼狈万分。一只鹦鹉见了，便问道：“狐狸先生，您这是怎么啦？”狐狸一脸惶恐地说：“后……后面有一大群猎犬在追我！”鹦鹉听了心急地大叫：“哎呀！那你赶快到村口玛丽大婶家里躲一躲吧。她最好，一定会收留你的。”狐狸一听，说：“玛丽大婶？不行，前两天我还偷了她的鸡，她不会收留我的。”鹦鹉想了想，又说：“没关系，史密斯大爷的家离这里也不远，你赶快跑到他那儿躲起来呀！”狐狸却说：“史密斯大爷也不行，几天前我趁他不在家时，偷吃了他孙女养的金丝雀，他们一家正痛恨我呢！”鹦鹉又说：“那么，你去投靠杰佛逊大夫吧，他是这村里惟一的医生，非常有爱心，一定不忍心看你被抓的。”狐狸尴尬地说：“那个杰佛逊大夫呀？上次我到他家里，把他存的肉片给吃得一干二净，还把他院子里种的郁金香给踩烂了……我没脸再去找他。”鹦鹉无奈地问：“难道这个村里就没有你可以投靠的人了吗？狐狸回答：“没有，我平时可没少害他们啊！”鹦鹉摇摇头，说：“唉，那么我也救不了你了。”最后，这只平日里耀武扬威的狐狸，被猎犬给抓住了。
      
【温馨提示】

      没有人会一生一帆风顺，没有人永远高枕无忧。当你失败时，还有没有愿意帮你的朋友？做一个好人，其实是在为自己留一条万一的后路。你做过一件坏事，可能要付出十倍的代价；同样地，你做过一件好事，也许会有十倍的回报──这就是利息。你平时怎样待人，将决定你失意时别人怎样待你；你失意时别人怎样待你，也决定了你的失败究竟是一败涂地还是有惊无险。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "加温和女巫",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在一次大学语言课上，老师给学生留了一个家庭作业：先阅读一篇文章，并思考提出的问题，等下一节课将各自思考的答案告诉大家。
      文章的大意是：年轻的亚瑟国王被邻国抓获。邻国的君主没有杀他，并承诺，只要亚瑟可以回答一个非常难的问题，他就可以给亚瑟自由。问题是：女人真正想要的是什么？”这个问题连最有见识的人都困惑难解，何况年轻的亚瑟。于是人们告诉他去请教一位老女巫，只有她才知道答案。女巫答应回答他的问题，但他必须首先接受她的交换条件。这个条件是：让自己和亚瑟王最高贵的圆桌武士之一、他最亲近的朋友──加温结婚。亚瑟王惊骇极了，他无法置信地看着女巫：驼背，丑陋不堪，只有一颗牙齿，浑身发出难闻的气味……亚瑟拒绝了，他不能因为自己让他的朋友娶这样的女人。加温知道这个消息后，对亚瑟说：“我同意和女巫结婚，对我来说，没有比拯救你的生命更重要的了。”于是婚礼举行了。女巫也回答了亚瑟的问题：女人真正想要的是可以主宰自己的命运。每个人都立即知道了女巫说出了真理，于是邻国的君主放了亚瑟王，并给了他永远的自由。
      来看看加温和女巫的婚礼吧，这是怎样的婚礼呀──为此，亚瑟王在无法解脱的极度痛苦中止不住地哭泣。加温一如既往地温文尔雅，而女巫却在婚礼上表现出最丑陋的行为：用手抓东西吃，蓬头垢面，用嘶哑的喉咙大声讲话。她的言行举止让所有的人都感到恶心。新婚的夜晚来临，加温依然坚强地面对可怕的处境。然而，走进新房，却被眼前的景象惊呆了：一个他从没见过的美丽少女半躺在婚床上！加温如履梦境，不知这到底是怎么回事。美女回答说：“因为当她是个丑陋的女巫时，加温对她非常体贴，于是她就让自己在一天的时间里一半是丑陋的，另一半是美丽的。”她问加温：“在白天和夜晚，你是想要哪一半呢？”多么残酷的问题呀！加温开始思考他的困境：是在白天向朋友们展现一个美丽的女人，而在夜里，在自己的屋子里，面对的是一个又老又丑如幽灵般的女巫？还是选择白天拥有一个丑陋的女巫妻子，但在晚上与一个美丽的女人共度过亲密的时光？故事结束了，问题是：如果你是加温，会怎样选择？
      第二天的课堂上，答案五花八门，归纳起来也就是两种：一种是选择白天是女巫，夜晚是美女，理由是妻子是自己的，不必爱慕虚荣，苦乐自知就可以了；一种选择白天是美女，因为可以得到别人羡慕的目光，至于晚上，漆黑的屋子，美丑都无所谓了。
      老师听了所有的答案，没有说什么，只是问大家是否想知道加温的回答，大家说当然想。老师说：“加温没有做任何选择，只是对他的妻子说，‘既然女人最想要的是主宰自己的命运，那么就由你自己决定吧！’”于是女巫选择──白天夜晚都是美丽的女人。
      所有的学生都沉默了：为什么我们没一个人做出加温那样的回答？
      
【温馨提示】

      有时我们是不是很自私？我们总以自己的喜好去安排别人的生活，却没有想过人家是不是愿意。如果对人多一些爱心、关怀，我们是不是也会像加温一样得到出乎意料的回报？",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "让失败改变方向",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在美国缅因州，有一个伐木工人叫巴尼?罗伯格。一天，他正在砍伐的大树突然倒下，右腿被沉重的树干死死压住，血流不止。面对自己伐木生涯中从未遇到过的失败和灾难，他的第一个反应就是：“我该怎么办？”此时此刻，他面临一个严酷的现实：周围几地里没有村庄和居民；10小时以内不会有人来救他；不久之后，他会因为流血过多而死亡。他不能等待，必须自己救自己。他用尽全身力气抽腿，可怎么也抽不出来。他摸到身边的斧头，开始砍树。但因为用力过猛，才砍了三四下，斧柄就断了。他向四周望了望，发现在不远的地方，放着他的电锯。他用断斧柄把最锯弄到手，想把压着腿的树干锯掉。可是，他发现树干是倾斜的，一旦拉动锯子，树干就会把锯条死死夹住。正当他几乎绝望的时候，他忽然涌出一个大胆的决定：把自己被压住的大腿锯掉！他当机立断，毅然锯断了自己的大腿，终于成功地拯救了自己的生命。
      
【温馨提示】

      一位哲学家面对一个失败者说过这样的话：“人生免不了失败。失败降临时，最好的办法是阻止它、克服它、扭转它，但多数情况下常常无济于事。那么，你就换一种思维，设法让失败改道，变在失败为小失败，在失败中寻找成功。”相对于死亡而言，仅仅失掉一条腿，何尝不是成功和胜利呢？",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "改变花瓶的颜色",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在巴黎，一位贵妇人的家中保存了一只祖传的珍稀花瓶。她非常钟爱这只花瓶，每天都要端详很多次。有一天，她想把卧室重新粉刷一次，为了协调，她决定让墙面采用花瓶的颜色。贵妇人找了好几个油漆匠，全都因为无法调出与花瓶完全相配的色调被辞退。贵妇人对此也感到非常气绥。最后，又来了一名油漆匠，他愿意尝试一下。经过一番努力，他把卧室粉刷得和花瓶的颜色丝毫不差，贵妇人非常满意，付给他一大笔工钱。不久，这名油漆匠就远近闻名了。
      几年后，这个油漆匠准备退休，他决定把手艺传给儿子。“爸爸，”有一天儿子问他，“有件事我很想知道，当年你是怎样把贵
      妇人的卧室刷得和花瓶的颜色安全一致的呢？”父亲看着儿子，然后慈祥地说：“孩子，这非常简单，我只是稍稍改变了一下花瓶的颜色。”
      让墙的颜色与花瓶的颜色一样，或者让花瓶的颜色与墙的颜色一样，对那个贵妇人来说，其实没有什么区别。
      
【温馨提示】

      如果我们不能让天空变小，我们不妨让我们的心灵变大。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "快乐的种子",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      上帝把一捧快乐的种了交给幸福之神，让她到人间去撒播。临行之前，上帝仍不放心地问：“你准备把它们撒在什么地方？”幸福之神胸有成竹地回答说：“我已经想好了，我准备把这样种子放在最深的海底，让那些寻找快乐的人，经过大海惊涛骇浪的考验后，才能找
      到它。”上帝听了，微笑着摇了摇头。幸福之神考虑了一会儿，继续说：“那我就把它们藏在高山之上吧，让寻找快乐的人，通过艰难跋涉才能发现它的存在。”上帝听了之后，还是摇了摇头。幸福之神茫然无措了。上帝意味深长地说：“你选择的两个地方都不难找到。你应该把快乐的种子撒在每个人的心底。因为，人类最难到达的地方，就是他们自己的心灵。”
      
【温馨提示】

      每个人都希望得到快乐，然而我们在寻找快乐时，往往看不见藏在自己心底的那粒种子，因为，我们的心里充塞了太多的忧虑、欲望、抱怨和仇恨。只要我们时时不忘撒进希望的阳光和真诚的雨露，那一粒藏在我们心中的快乐种子，即使我们没有看见，它也会自己生根发芽。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "拿破仑最后的失败",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      滑铁卢战役是拿破仑一生最后的失败吗？不是。拿破仑的最后失败是败在一枚棋子上。
      滑铁卢失败后，拿破仑被终身流放到圣赫勒拿岛。他在岛上过着十分艰苦而无聊的生活．一位密友听说此事，通过秘密方式赠给拿破仑一件珍贵的礼物──一副象棋。这是一副用象牙和软玉制成的国际象棋，拿破仑对它爱不释手。在非常寂寞时，他就一个人默默地下棋，借以打发时光，直到死去。
      拿破仑死后，那副象棋被多次以高价转手拍卖。在一次偶然的机会中人们发现，这副象棋中一枚棋子的底部可以打开。当它被打开后，一个惊人的秘密被发现了：棋子里藏有一张手绘的地图，上面详细标注着如何从圣赫勒拿岛逃出的路径。可惜的是，拿破仑自始至终都没有想到过逃跑，所以也没有领悟到朋友的良苦用心，也就没有在玩乐中发现这一奥秘。这恐怕是拿破仑一生中最大的失败。
      拿破仑一生转战南北，心机算尽，用许多常人料想不到的方法，征服了一个个国家。但是他怎么也没有想到，最后竟然死在了常规思维上。如果他用作战的方法思考一下象棋解除寂寞之外的用意，很可能就会发现象棋中巨大的秘密。
      也许我们的推断根本就是错误的。拿破仑失败并不在于他没有使用超常的思维来对待这副象棋，其实，他失败的根本原因是──他根本没想到能逃跑！如果他想到了，即使没有这副象棋，心计万千的拿破仑同样可以有别的办法。
      
【温馨提示】

      心已经放弃了，还谈何胜利？",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "布勃卡的意念‘跳’",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      布勃卡是举世闻名的奥运会撑杆跳冠军，享有“撑杆跳沙皇”的美誉。他曾35次创造撑杆跳世界纪录，所保持的两项世界纪录，迄今无人打破。在参加“国家勋章”的授勋典礼上，记者们纷纷提问：“你的成功的秘诀是什么？”布勃卡微笑着说：“很简单，每次撑杆跳之前，我先让自己的意念‘跳’过横杆。”
      作为一名撑杆跳选手，有一段日子，尽管布勃卡不断尝试新的高度，但每次都以失败告终。他苦恼过、沮丧过，甚至怀疑自己的潜力。有一天，他来到训练场，禁不住摇头对教练说：“我实在跳不过去。”
      教练平静地问：“你是怎么想的？”
      布勃卡如实回答：“只要踏上起跳线，一看见那根高悬的横杆，心里就害怕。”
      教练看着他，突然厉声喝道：“布勃卡，你现在要做的是闭上眼睛，先让你的意念从标杆上‘跳’过去。”
      教练的训斥，让布勃卡如梦初醒。遵从教练的吩咐，他重新撑杆。这一次，他顺利地跃身而过。教练欣慰地笑了，语重心长地说：“记住，先将你的意念从标杆上‘跳’过去，你的身体就一定会跟着过去。”
      
【温馨提示】

      突破心灵障碍，才能超越自己。如果你的意念屈服了，那么你可能真的就不行。著名的钢铁大王卡耐基经常提醒自己的一句箴言是：我想赢，我一定能赢。结果他真的赢了。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "黄金距离三英寸",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一则故事曾在世界各地的淘金者中广为传诵。这个故事有着一个极其动听的名字，叫做“黄金距离三英寸”。
      几十年前，家住马里兰州的达比和他叔叔一起到遥远的美国西部去淘金，他们手握鹤嘴镐和铁锹不停地挖掘，几个星期后，终于惊喜地发现了金灿灿的矿石。于是，他们悄悄将矿井掩盖起来，回到家乡的威廉堡，筹集大笔资金购买采矿设备。不久，淘金的事业便如火如荼地开始了。当采掘的首批矿石运往冶炼厂时，专家们断定，他们遇到的可能是美国西部罗拉地区藏量最大的金矿之一。达比仅仅只用了几车矿石，便很快将所有的投资全部收回。让达比万万没有料到的是，正当他们的希望在不断膨胀的时候，奇怪的事发生了：金矿的矿脉突然消失！尽管他们继续拼命地钻探，试图重新找到金矿石，但一切终归徒劳，好像上帝有意要和达比开一个巨大的玩笑，让他的美梦成为泡影。万般无奈之际，他们不得不忍痛放弃了几乎要使他们成为新一代富豪的矿井。接着，他们将全套机器设备卖给了当地一个收购废旧品的商人，带着满腹遗憾回到了家乡威廉堡当起了小农场主。就在他们刚刚离开后的几天里，收废品的商人突发奇想，决定去那口废弃的矿井碰碰运气。为此，他还专门请来一名采矿工程师，只做了一番简单的测算，工程师便指出，前一轮工程失败的原因是由于业主不熟悉金矿的断层线。考察结果表明，更大的矿脉距离达比停止钻探的地方只有三英寸！
      故事的结果是，达比终其一生只是一名收入仅够养家的小农场主，而这位从事废品收购的小商人，终于成为西部巨富。达比虽然付出了最大的努力，但他获取的却是罗拉地区最大金矿的一个小小支脉；收废品的商人虽然只花费了很小的代价，却通过一口废弃的矿井而成功地拥有了最大金矿的全部。
      
【温馨提示】

      前者是一种命运，后者也是一种命运。这两种截然不同的命运背后，原本暗藏着一次完全相同的机遇。不同的是，面对“失败”和“不可能”，一个轻易放弃了，而另一个却敢于去尝试一次。记住这个教训：黄金距离三英寸。世上的很多“不可能”都是一种考验人的表象，只有大胆的“尝试”可以改变它。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "青春的秘密",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      日本许多商界要人，都喜爱一篇短短的散文，散文的题目叫《青春》，作者塞缪尔?厄尔曼。此人l840年生于德国，儿时随家人移居美利坚，参加过南北战争，之后定居伯明翰，经营五金杂货，年逾70开始写作。
      《青春》一文，仅寥寥400字：
      青春不是年华，而是心境；青春不是桃面、丹唇、柔膝，而是深沉的意志、恢宏的想像、炽热的感情；青春是生命的深泉涌流。
      青春气贯长虹，勇锐盖过怯弱，进取压倒苟安。如此锐气，二十后生有之，六旬男子则更多见。年岁有加，并非垂老；理想丢弃，方堕暮年。
      岁月悠悠，衷微只及肌肤；热忱抛却，颓唐必致灵魂。忧烦、惶恐、丧失自信，定使心灵扭曲，意气如灰。
      无论年届花甲，抑或二八芳龄，心中皆有生命之欢乐，奇迹之诱惑，孩童般天真久蛊不衰。
      人的心灵应如浩淼瀚海，只有不断接纳美好、希望、欢乐、勇气和力量的百川，才能青春永驻、风华长存。
      一旦心海枯竭，锐气便被冰雪覆盖，玩世不恭、自暴自弃油然而生，即使年方二十，实已垂垂老矣；然则只要虚怀若谷，让喜悦、达观、仁爱充盈其间，你就有望在八十高龄告别尘寰时仍觉年轻。
      此文一出，不径而走，以至代代相传。二战期间，麦克阿瑟与日军角逐于太平洋时，将此文镶于镜框，摆在写字台上，以资自勉。
      日本战败，此文由东京美军总部传出，有人将它灌成录音带，广为销售；甚至有人把它揣在衣兜里，随时研读。
      多年后，厄尔曼之孙、美国电影发行协会主席乔纳斯?罗森菲尔德访问日本，席间谈及《青春》一文，一位与宴者随手掏出《青春》，恭敬地说：“乃翁文章，鄙人总不离身。”主客皆万分感动。
      1988年，日本数百名流聚会东京、大阪，纪念厄尔曼的这篇文章。松下电器公司元老松下幸之助感慨的说：“20年来，《青春》与我朝夕相伴，它是我的座右铭。”欧洲一位政界名宿也极力推荐：“无论男女老幼，要想活得风光，就得拜读《青春》。”
      厄尔曼并非哲人，名不见经传，但《青春》一文，却拨动了不少人的心弦，使人如听晨钟，如闻幕鼓，朝夕自警自策。只因为它真正说出了关于青春的秘密。
      
【温馨提示】

      青春不是年华，而是心境。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "你是全力以赴还是尽力而为？",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一天猎人带着猎狗去打猎。猎人一枪击中一只兔子的后腿，受伤的兔子开始拼命地奔跑。猎狗在猎人的指示下也是飞奔去追赶兔子。可是追着追着，兔子跑不见了，猎狗只好悻悻地回到猎人身边，猎人开始骂猎狗了：“你真没用，连一只受伤的兔子都追不到！”猎狗听了很不服气地回道：“我尽力而为了呀！”
      再说兔子带伤跑回洞里，它的兄弟们都围过来惊讶地问它：“那只猎狗很凶呀！你又带了伤，怎么跑得过它的？”“它是尽力而为，我是全力以赴呀！它没追上我，最多挨一顿骂，而我若不全力地跑我就没命了呀！”
      
【温馨提示】

      人本来是有很多潜能的，但是我们往往会对自己或对别人找借口：“管它呢，我们已尽力而为了。”事实上尽力而为是远远不够的，尤其是现在这个竞争激烈的年代。让我们常常问问自己：“我今天是尽力而为的猎狗，还是全力以赴的兔子呢？”",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "一只破水桶的启示",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位农夫有两只水桶，他每天就用一根扁担挑著两只水桶去河边汲水。
      两只水桶中有一只有一道裂缝，因此每次到家时这只水桶总是会漏得只剩下半桶水，而另一只桶却总是满满的。就这样，两年以来，日复一日，农夫天天只能从河里担回家一桶半水。
      完整无缺的桶很为自己的完美无缺得意非凡，而有裂缝的桶自然为自己的缺陷和不能胜任工作而羞愧。经过两年的失败之后，一天在河边，有裂缝的桶终于鼓起勇气向主人开了口：“我觉得很惭愧，因为我这边有裂缝，一路上漏水，只能担半桶水到家。”
      农夫回答它说：“你注意到了吗？在你那一侧的路沿上开满了花，而另外的一侧却没有花？我从一开始就知道你有漏，于是在你的那一侧的路沿撒了花籽。我们每天担水回家的路上，你就给它们浇水。两年了，我经常从这路边采摘鲜花来装扮我的餐桌。如果不是因为你的所谓的缺陷，我怎么会有美丽的鲜花装扮我的家呢？”
      
【温馨提示】

      我们每个人都好比那只有裂缝的桶，各自都具有这样或那样的不足和缺点。倘若我们怀著一颗包容的心，懂得发现对方的长处，并且能够扬长避短，我们的生活一定会变得更加轻松愉快和丰富多彩。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "相信自己是一只雄鹰",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个人在高山之巅的鹰巢，抓到了一只幼鹰，他把幼鹰带回家，养在鸡笼。这只幼鹰和鸡一起啄食、嬉闹和休息。它以为自己是一只鸡。这只鹰渐渐长大，羽翼丰满了，主人想把它训练成猎鹰，可是由於终日和鸡混在一起，它已经变得和鸡完全一样，根本没有飞的愿望了。主人试了各种办法，都毫无效果，最后把它带到山顶上，一把将它扔了出去。这只鹰像块石头似的，直掉下去，慌乱之中它拼命地扑打翅膀，就这样，它终於飞了起来！
      
【温馨提示】

      磨练召唤成功的力量！",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "狮子和羚羊的家教",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      每天，当太阳升起来的时候，非洲大草原上的动物们就开始奔跑了。狮子妈妈在教育自己的孩子：“孩子，你必须跑得再快一点，再快一点，你要是跑不过最慢的羚羊，你就会活活地饿死。”
      在另外一个场地上，羚羊妈妈也在教育自己的孩子：“孩子，你必须跑得再快一点，再快一点，如果你不能比跑得最快的狮子还要快，那你就肯定会被他们吃掉。”
      
【温馨提示】

      记住你跑得快，别人跑得更快。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "看看谁更聪明呢?",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      美国第九届总统威廉?享利?哈里逊出生在一个小镇上，他是一个很文静又怕羞的孩子，人们都把他看作傻瓜，常喜欢捉弄他。他们经常把一枚五分硬币和一枚一角硬币扔在他面前，让他任意捡一个，威廉总是捡那个五分的，于是大家都嘲笑他。有一天一个好心人可怜地问道：“难道你不知道一角钱要比五分钱值钱吗？”“当然知道，”威廉慢条斯理地说：“不过，如果我捡了那个一角的，恐怕就没人有兴趣扔钱给我了。”
      
【温馨提示】

      小朋友们都喜欢和别人竞争，谁也不喜欢让着谁。看了这个故事，你就明白了，有时候退一步可以得到的更多，正所谓“退一步海阔天空”！",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "飞蛾的痛苦经历",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      生物学家说，飞蛾在由蛹变茧时，翅膀萎缩，十分柔软；在破茧而出时，必须要经过一番痛苦的挣扎，身体中的体液才能流到翅膀上去，翅膀才能充实有力，才能支持它在空中飞翔。
      一天有个人凑巧看到树上有一只茧开始活动，好象有蛾要从里面破茧而出，于是他饶有兴趣地准备见识一下由蛹变蛾的过程。但随着时间的一点点过去，他变得不耐烦了，只见蛾在茧里奋力挣扎，将茧扭来扭去的，但却一直不能挣脱茧的束缚，似乎是再也不可能破茧而出了。最后，他的耐心用尽，就用一把小剪刀，把茧上的丝剪了一个小洞，让蛾出来可以容易一些。果然，不一会儿，蛾就从茧里很容易地爬了出来，但是那身体非常臃肿，翅膀也异常萎缩，耷拉在两边伸展不起来。他等着蛾飞起来，但那只蛾却只是跌跌撞撞地爬着，怎么也飞不起来，又过了一会儿，它就死了。
      原来,那“鬼门关”般的狭小茧洞恰是帮助飞蛾幼虫两翼成长的关键所在,穿越的时候,通过用力挤压,血液才能顺利送到蛾翼的组织中去，惟有两翼充血，飞蛾才能振翅飞翔。人为地将茧洞剪大，飞蛾的翼翅就失去充血的机会，生出来的飞蛾便永远与飞翔绝缘。
      
【温馨提示】

      “不经历风雨，怎能见彩虹”，任何一种本领的获得都要经由艰苦的磨练，“梅花香自苦寒来，宝剑锋从磨砺出。”任何投机取巧或妄图减少奋斗而达到目的的做法都是见识短浅的行为，那只飞不起来的飞蛾的经历就证明了这一切。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "狮子与野牛",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      森林里，一头饥饿的狮子正在寻找食物，当它发现了一群正在吃草的野牛时，狮子像看到了救命稻草，两眼放光，狡猾的狮子塌腰沉背，借助青草的掩护，悄悄地向野牛靠近。狮子离野牛越来越近，野牛们却丝毫没有感觉到危险，仍悠然地啃食着青草。狮子进入了捕捉范围，它跃身而起，箭一般射向野牛群。野牛惊慌奔逃。狮子紧追着一头还未长结实的野牛。野牛拼命奔逃着，每当狮子要追上它时便拐个弯，就可和狮子拉开一段距离。可没多久，野牛就体力不支了。狮子和野牛的距离在不断缩小。突然，野牛刹住脚，猛转过身，将锋利的犄角转向狮子。狮子也停下了脚步，左右试探着寻找进攻的机会，可野牛的犄角始终对着它。狮子和野牛对峙着，僵持着。突然，野牛向前迈进了一步，狮子竟后退了一步，并仰躺下，四脚朝天，宛如一只小猫，只是眯起的眼睛仍牢牢盯着野牛。狮子的“懦弱”让野牛顿生豪气，野牛用犄角猛扎着躺在地上的狮子，但是狮子的血口利齿已紧咬在野牛的喉咙上，威猛进攻的野牛硕壮的身体慢慢倒了下去，没有丝毫挣扎。
      
【温馨提示】

      狮子不仅胜在利齿，还有狡黠；野牛不仅败在没有利齿，还有被蒙骗后的盲目自大。勇敢是把利刃可以斩落对手；自大也是把利刃，却只能割伤自己。危急时刻，平衡心态，不骄傲自大，才会成功。",
            group6));

            group6.Items.Add(new SampleDataItem("Group-4-Item-3",
            "蜘蛛与网",
            "Item Subtitle: 3",
            "Assets/title/6.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      黄昏，屋檐下，有一只小蜘蛛正精心地织着网，这是它离开妈妈独自编的第一张捕虫网啊！一会儿工夫，一张网编织成功了，小蜘蛛叭在网中央，忽忽悠悠，兴奋得不得了。谁知好景不长，一只麻雀飞来，不偏不倚，一下把网给撞了个稀巴烂。小蜘蛛心痛死了，没办法，只好重新拉丝结网。它织呀织呀，不久，一张比前一张更坚固、更美观的网又完成了。真是“人在家中坐，祸从天上来”，正当小蜘蛛庆祝胜利的时候，突然刮来一阵旋风，瞬间把网刮得无影无踪。这回，小蜘蛛的眼泪再也止不住了，它伏在椽头上，失声地大哭起来。小蜘蛛的哭声，申惊动了这里的老住户——一只年迈的蜘蛛奶奶，她问明原委，擦干小蜘蛛脸上的泪水，语重心长地说：“孩子，这点磨难算什么呀，哪一只蜘蛛的网不破上几百次呢，正因为有了那几百次失败的教训，我们才获得了当之无愧的‘纺织专家’的桂冠啊！孩子，振作起来，从头开始吧！”
      小蜘蛛呢？你知道的，它一直在织着网，那房前屋后，总有它的作品在展出哩。
      
【温馨提示】

      面对困难要永不放弃。",
            group6));




            this.AllGroups.Add(group6);
            #endregion

            #region 第7章
            var group7 = new SampleDataGroup("Group-7",
            "第7章",
            "Group Subtitle: 2",
            "Assets/title/7.png",
            "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
"鱼竿和鱼",
"Item Subtitle: 3",
"Assets/title/7.png",
"Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
@"
      有个老人在河边钓鱼，一个小孩走过去看他钓鱼，老人技巧纯熟，所以没多久就钓了满篓的鱼，老人见小孩很可爱，要把整篓的鱼送给他，小孩摇摇头，老人惊异的问道：“你为何不要？”小孩回答：“我想要你手中的钓竿。”老人问：“你要钓竿做什么？”小孩说：“这篓鱼没多久就吃完了，要是我有钓竿，我就可以自己钓，一辈子也吃不完。”
      我想你一定会说：“好聪明的小孩。”错了，他如果只要钓竿，那他一条鱼也吃不到。
      
【温馨提示】

      他不懂钓鱼的技巧，光有鱼竿是没用的，因为钓鱼重要的不在钓竿，而在钓技。有太多人认为自己拥有了人生道上的钓竿，再也无惧于路上的风雨，如此，难免会跌倒于泥泞地上。",
group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "渔王和他的儿子",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有个渔人有着一流的捕鱼技术，被人们尊称为‘渔王’。然而‘渔王’年老的时候非常苦恼，因为他的三个儿子的渔技都很平庸。
      于是个经常向人诉说心中的苦恼：“我真不明白，我捕鱼的技术这么好，我的儿子们为什么这么差？我从他们懂事起就传授捕鱼技术给他们，从最基本的东西教起，告诉他们怎样织网最容易捕捉到鱼，怎样划船最不会惊动鱼，怎样下网最容易请鱼入瓮。他们长大了，我又教他们怎样识潮汐，辨鱼汛......凡是我长年辛辛苦苦总结出来的经验，我都毫无保留地传授给了他们，可他们的捕鱼技术竟然赶不上技术比我差的渔民的儿子！”
      一位路人听了他的诉说后，问：“你一直手把手地教他们吗？”
      “是的，为了让他们得到一流的捕鱼技术，我教得很仔细很耐心。”
      “他们一直跟随着你吗？”
      “是的，为了让他们少走弯路，我一直让他们跟着我学。”
      路人说：“这样说来，你的错误就很明显了。你只传授给了他们技术，却没传授给他们教训，对于才能来说，没有教训与没有经验一样，都不能使人成大器！”
      
【温馨提示】

      看来有了鱼篓、鱼竿、渔技还不能成大器，知其然不知其所以然也，这就是“喂食”和“觅食”的差别吧。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "让好习惯主宰人生",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      亚历山大帝王图书馆发生火灾的时候，馆里所藏图书被焚烧殆尽，但有一本不很贵重的书得以幸免。有一个能识几个字的穷人，花了几个铜板买下了这本书。书本身不是很有意思，但书页里面却藏着一样非常有趣的东西：一张薄薄的羊皮纸，上面写着点铁成金石的秘密。所谓点铁成金石，是一块小圆石，能把任何普通的金属变成纯金。小纸片上写着：这块奇石在黑海边可以找到，但是奇石的外观跟海边成千上万的石头没什么两样。谜底在于：奇石摸起来是温的，而普通的石头摸起来是冰凉的。这个穷人于是变卖了家当，带着简单的行囊，露宿于黑海岸边，开始寻找点铁成金石。
      他知道，如果他把捡起来的冰凉的石头随手就扔掉的话，那么他可能会重复地捡到已经摸过的石头，而无法辨认真正的奇石。为防止这种情形的发生，每当捡起一块冰凉的石头，他就往海里扔。一天过去了，他捡的石头中没有一块是书中所说的奇石。一个月，一年，二年，三年……他还是没找到那块奇石。但是，他不气馁，继续捡石头，扔石头……没完没了。有一天早上，他捡起一块石头，一摸，是温的！他仍然随手扔到了海里，因为他已经养成了往海里扔石头的习惯。这个扔石头的动作太具习惯性了。以至于当他梦寐以求、苦苦寻觅的奇石出现时，他仍然习惯性地扔到了海里。
      
【温馨提示】

      英国教育家洛克说：“习惯一旦养成之后，便用不着借助记忆，很容易很自然地就能发生作用了。”事实确实是这样的。就拿那个穷人来说，他多少年风餐露宿，苦苦寻觅，为的就是那块点铁成金石。可是当他找到后，他却随手扔到了海里。不是他不想要那块奇石，而是往海里扔石头的习惯性动作迫使他做出了令人遗憾不已的蠢事。他的多年点铁成金梦，也像肥皂泡一样顷刻破灭了。正如培根所说“习惯真是一种顽强而巨大的力量。它可以主宰人生。”他的话就像是对那个穷人说的似的，准确，深刻！然而我们应该深刻认识它的警示意义。要拥有美好人生，必须养成一种好的习惯，让它服务于我们。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "竹篮打水不会一场空",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位老人和他的孙子生活在一个山区庄园里。每天清晨，爷爷都坐在厨房的桌子旁阅读书籍。孙子受爷爷的影响，也尽最大的努力，阅读那些书籍。一天，孙子问：“爷爷，我一直试图像你一样阅读这些好书，但我不能真正理解它。花费这么多时间读这些书，有什么用呢？”爷爷平静地拿出一个用竹子编成的放过煤的竹篮，对孙子说：“拿着这个竹篮，取一篮子水来。”孙子提着篮子走了。等他回到家中，竹篮里的水一滴不剩。爷爷笑着对孙子说：“下一次打水的时候，你必须跑得更快点。”孙子再次去打水，这次他跑得比上次快了许多。但是，结果依然不变。孙子告诉爷爷：“用竹篮打水是不可能完成任务的。”说着，孙子换了一个桶过来。爷爷说：“我不需要一桶水，我要一篮子水。你能够做到，只是你尝试得还不够充分。”爷爷走出来，亲眼看孙子去打水。
      孙子清楚地知道，竹篮打水是根本不可能的，但他希望给爷爷演示一遍，让爷爷知道，即使他尽最大努力，篮子里的水也会漏光。孙子盛满水，飞快地向爷爷跑来，篮子中空空如也。孙子气喘吁吁地对爷爷说：“爷爷，你看，根本没有一点用。”“你真的认为这样做没有一点用吗？”爷爷说，“好好看看这篮子。”孙子仔细看了一下竹篮，竹篮确实和以前不同了，那个脏兮兮、黑糊糊的篮子不见了，取而代之的是一个洁净如新的竹篮。
      “孩子，看到发生的事情了吧，阅读好书也是哪些。你可能无法完全理解，也记不住多少内容，但只要你用心阅读，它就会在不知不觉间净化你的心灵。”
      
【温馨提示】

      知识需要积累，持之以恒是会有收获的。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "忘记那只打碎的“茶壶”",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      曾听说过一个故事，说有一个担着两筐茶壶去集市上卖，谁知他在经过一个山坡时，几只茶壶从筐里掉出来，摔了个粉碎。他头也不回地向前走，这时有人提醒他说：“喂，你的茶壶摔碎了，你还不快看看！”这人回答说：“既然已经摔碎了，看又有什么用呢？”
      这个人堪称是一位智者，因为在生活中能像他一样生活的实在不多。我们见到更多的人是患得患失、斤斤计较，面对已经打碎的“茶壶”，叹息一番，后悔不已，结果白白浪费了时间不说，甚至因为分神，弄不好还会打碎更多的“茶壶”。
      对人生而言，随时都会有意想不到的事情发生，有时远远比打碎一只茶壶更为严重。人人都渴望成功，然而成功并非是唾手可得的，它不要经历许多的挫折和失败，才有望实现。当你面对一些不幸或者打击时，你能不能潇洒地挥一挥手，告别昨天，从头再来，已显得至关重要。
      世界著名话剧演员波尔赫德，在世界戏剧舞台上活跃了五十多年，当也71岁，因一场意外的事件导致她破产了，而更糟糕的是，她的腿部受伤引起脉管炎，不得不截肢。当医生忐忑不安地把这个消息告诉她时，她却平静地说：“既然没有别的办法，那就面对现实吧。”手术那天，她在手术台上高声朗诵戏剧台词，别人问她是否在安慰自己，她说：“不，我是在安慰医生和护士。”后来，波尔赫德又继续在舞台上演出了7年。
      爱迪生67岁那年，一场大火残酷无情地烧毁了他的研制工厂，次日早晨，爱迪生踩在那百万资产的废墟上，乐观地说：“现在，我们又可以一切重新开始了。”
      
【温馨提示】

      当人身处逆境时，最重要的是培养适应环境的能力。应该知道，通向成功的路不止一条，没必要一条路走到黑。忘掉那些恼人的事并不意味着背叛了你的初衷。放弃无可挽回的事也并不说明你整个人生从此就暗淡无光了。须知，上帝在向你关闭一扇门的同时，也向你敞开了另一扇窗，你完全可以以退为进，以守为攻，一旦时机成熟，你就会反败为胜。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "猎狗和土拨鼠",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      老师给学生讲了一个故事：有三只猎狗追一只土拨鼠，土拨鼠钻进了一个树洞。这只树洞只有一个出口，可不一会儿，从树洞里钻出一只兔子。兔子飞快地向前跑，并爬上一棵大树。兔子在树上，仓皇中没站稳，掉了下来，砸晕了正仰头看的三只猎狗，最后，兔子终于逃脱了。
      故事讲完后，老师问：“这个故事有什么问题吗？”学生说：“兔子不会爬树。”，“一只兔子不可能同时砸晕三只猎狗。”“还有哪？”老师继续问。直到学生再找不出问题了，老师才说：“可是还有一个问题，你们都没有提到，土拨鼠哪里去了？”
      
【温馨提示】

      在追求人生目标的过程中，我们有时也会被途中的细枝末节和一些毫无意义的琐事分散精力，扰乱视线，以至中途停顿下来，或是走上岔路，而放弃了自己原先追求的目标。不要忘了时刻提醒自己，土拨鼠哪去了？自己心目中的目标哪去了？别让目标受到干扰！",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "能力和学历",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      请你用一句最简洁的话，回答下面四位著名人士到底在说些什么。
      1、1954年4月2日，苏黎世联帮工业大学建校100周年，邀请爱因斯坦回母校演讲，爱因斯坦在演讲中说了这么几句话：我学习成绩中等，按学校的标准，我算不上是个好学生，不过后来我发现，能忘掉在学校的东西，剩下的才是教育。
      2、1984年10月6日，诺贝尔物理学获得者丁肇中回母校清华大学演讲，在接受学生的提问时说了这么一句：据我所知，在获得诺贝尔奖的90多位物理学家家，还没有一位在学校里经常老第一，经常考倒数第一的倒有几位。
      3、1999年3月21日，比尔?盖茨应邀回母校哈佛大学参加募捐会，在记者问他是否愿意继续学习、拿到哈佛的毕业证书时，盖茨向那位记者笑了一下，没有回答。
      4、2001年5月21日，美国总统布什返回母亲耶鲁大学，接受荣誉法学博士学位。由于当年他成绩平平，在被问到现在接受这项荣誉作何感想时，他说：“对好些取得优异成绩的毕业生，我说‘干得好’；对那些成绩较差的毕业生，我说‘你可以去当总统’。”
      这是巧克力之父费斯?贝里经营的公司在登陆中国时，给求职者出的问卷。公开开始发出招聘信息时就收到了400多份自荐信，弗斯?贝里看到那么多求职信时，非常高兴，可是他在阅读了这些信件后，却犹豫起来，因为在这400多份自荐信中，有300多人的学习成绩每科都在90分以上，并且有80%以上的学生曾担任过学生会干部，从老师给他们写的评语看，每个学生的在校表现也都是尽善尽美的。弗斯?贝里读完自荐信，没有对自荐者的诚信产生怀疑，他相信这一切都是真的。中国是一个重视教育的国家，中国学生无论在哪个国家读书，成绩都是出了名的优秀。不过，他觉得仅凭这些还不确定谁有资格进入他的公司。他想，要在这些好学生中选一位适合自己公司的人，还必须测试点其它的东西，于是他出了以上的那份问卷。
      接到回执的400多名同学，均发回自己的答案。2003年3月10日，乔治王巧克力公司中国分公司在北京开业，有一位学生被通知参加开业庆典。他是这么回答的：学校里有高分低分之分，但校门外没有，校门外总是把校门里的一切打乱重排。
      
【温馨提示】

      一个人在学校里学习成绩差，并不代表着走上社会后的工作和创业能力差。在校门外，主要看的不是学历，而是能力。有时，在校园里学到的东西，在校门外可能用不上。校门外和校门内是有差别的，对于环境的改变，我们的心态也要跟着改变。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "木匠与树",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个木匠前往一个城镇，在路上他看见一座土地庙的旁边有一棵高大无比的栎树。这是一棵巨大无比的栎树，它的树阴可供几千头牛在树乘下凉；树干又粗又直，在几丈高之后才能见到分枝，，而这些树枝粗到可以用来做造船材料的就有好几十枝。许多路人都在围观，连声称奇，只有这个木匠瞄了一眼，扭头就走。
      同路人好奇地问道：“这是一棵多么高大华美的树木，你怎么看都不看一眼就走了呢？”木匠平静地说：“这棵树没什么用。用来造船，船会沉；做棺材，棺材会腐烂；做器具，器具会破裂；做门窗，门窗会流出汁液；做柱子，柱子会被虫蛀。正是因为它没有用，才会长这么长寿，这么高。”
      很奇怪，晚上，木匠竟然梦见这棵大树对他说：“你怎么能说我没用呢？我想想看，那些所谓的橘树、梨树和柚树，要果实成熟时，就会被人拉扯攀折，很快就会死掉。一切有用的东西无不如此。你眼中的无用，对我来说，正是大用。假如我像你所说的那样有用，岂不早就被砍了吗？”
      木匠醒来，若有所悟。他把这个梦和昨天的事告诉了妻子。妻子问道：“它既然向往无用，为什么要长在土地庙旁边，引人注意呢？”木匠答道：“如果它不是长在庙旁边，而是长在路中央，不也早被人砍掉当柴烧了吗？”
      
【温馨提示】

      世间没有绝对的标准。人们通常所谓的标准，都是人为制定的，它会随着时间以及社会的发展有所改变。千万不要用一把绝对的尺子作为衡量的标准。在学习上敢于质疑，敢于突破，才能取得更大的进步。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "功——就是将简单的事情重复做",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      著名的推销大师，即将告别他的推销生涯，应行业协会和社会各界的邀请，他将在该城中最大的体育馆，做告别职业生涯的演说。
      那天，会场座无虚席，人们在热切地、焦急地等待着，那位当代最伟大的推销员，做精彩的演讲。当大幕徐徐拉开，舞台的正中央吊着一个巨大的铁球。为了这个铁球，台上搭起了高大的铁架。
      一位老者在人们热烈的掌声中，走了出来，站在铁架的一边。他穿着一件红色的运动服，脚下是一双白色胶鞋。人们惊奇地望着他，不知道他要做出什么举动。
      这时两位工作人员，抬着一个大铁锤，放在老者的面前。主持人这时对观众讲：请两位身体强壮的人，到台上来。好多年轻人站起来，转眼间已有两名动作快的跑到台上。老人这时开口和他们讲规则，请他们用这个大铁锤，去敲打那个吊着的铁球，直到把它荡起来。
      一个年轻人抢着拿起铁锤，拉开架势，抡起大锤，全力向那吊着的铁球砸去，一声震耳的响声，那吊球动也没动。他就用大铁锤接二连三地砸向吊球，很快他就气喘吁吁。另一个人也不示弱，接过大铁锤把吊球打得叮当响，可是铁球仍旧一动不动。台下逐渐没了呐喊声，观众好像认定那是没用的，就等着老人做出什么解释。
      会场恢复了平静，老人从上衣口袋里掏出一个小锤，然后认真地，面对着那个巨大的铁球。他用小锤对着铁球'咚'敲了一下，然后停顿一下，再一次用小锤'咚'敲了一下。人们奇怪地看着，老人就那样'咚'敲一下，然后停顿一下，就这样持续地做。
      十分钟过去了，二十分钟过去了，会场早已开始骚动，有的人干脆叫骂起来，人们用各种声音和动作发泄着他们的不满。老人仍然一小锤一停地工作着，他好像根本没有听见人们在喊叫什么。人们开始忿然离去，会场上出现了大块大块的空缺。留下来的人们好像也喊累了，会场渐渐地安静下来。
      大概在老人进行到四十分钟的时候，坐在前面的一个妇女突然尖叫一声：'球动了！'刹时间会场立即鸦雀无声，人们聚精会神地看着那个铁球。那球以很小的摆度动了起来，不仔细看很难察觉。老人仍旧一小锤一小锤地敲着，人们好象都听到了那小锤敲打吊球的声响。吊球在老人一锤一锤的敲打中越荡越高，它拉动着那个铁架子'哐、哐'作响，它的巨大威力强烈地震撼着在场的每一个人。终于场上爆发出一阵阵热烈的掌声，在掌声中，老人转过身来，慢慢地把那把小锤揣进兜里。
      老人开口讲话了，他只说了一句话：在成功的道路上，你没有耐心去等待成功的到来，那么，你只好用一生的耐心去面对失败。
      
【温馨提示】

      世间没有绝对的标准。人们通常所谓的标准，都是人为制定的，它会随着时间以及社会的发展有所改变。千万不要用一把绝对的尺子作为衡量的标准。在学习上敢于质疑，敢于突破，才能取得更大的进步。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "羊与狼",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      山顶上，狼吃了一只羊，恰好被狐狸看见了，它扯开嗓子大喊起来。它本来要喊的是:“羊被狼吃了！”但发生了口误，喊成了：“狼被羊吃了！”风儿把狐狸的话吹遍了山林。羊群听到喊声，精神大振。它们说:“不知哪位同胞给我们羊出了气、争了光，看来狼并不可怕！我们还等什么？冲上去，找狼算总账！”羊群潮水般地向狼发起了攻击。同时，狼群也听到了狐狸的喊声，它们一起愣住了：“这是真的吗？如果是真的，那也太可怕了！如果不是真的，狐狸为什么说得如此肯定呢？”就在它们六神无主的时候，大批红了眼的羊冲到狼群跟前。狼群惊慌失措，撒腿四处奔逃。
      山林中奇特的游戏很快结束了，羊和狼后来也都知道了真相。它们分别谈了自己的感想。羊说:“胜利的消息无疑会激励斗志，即使这个消息并不确切。否则，我们怎么会向狼发动攻击并取得胜利呢？”狼说:“我们过于相信自己的耳朵，否则，我们怎么会蒙受如此奇耻大辱？”
      
【温馨提示】

      世间没有绝对的标准。人们通常所谓的标准，都是人为制定的，它会随着时间以及社会的发展有所改变。千万不要用一把绝对的尺子作为衡量的标准。在学习上敢于质疑，敢于突破，才能取得更大的进步。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "聪明不是最重要的",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      要成功，聪明不是最重要的，成功要素排名谱上聪明只占第五位。那么，排在聪明前面的四位又是什么呢？
      第一，目标。你永远不可能按着一头不想喝水的牛去喝水，同样，你永远也不可能在你自己不热爱的领域，做出此生你所能作的最大成就。目标是欲望的表达，“要什么”从来就比“怎样做”更为重要。但是目标不是欲望。目标从来就具体，也往往给自己定了时限。换句话说，目标是具体化了的、有时间限制的目标。因而它既有欲望的感情、牵动因素，同时也有自己做主，不让自己从散漫中游移的因素。
      第二，胸怀。所谓胸怀，就是一股用天下之材，尽天下之利的气度，当然，还包括相当程度的包容--对异己的包容，对陌生的包容，对不如己者的包容。只有这样，你才会形成一种从广大处寻觅人生的态度，把生命的境界做大，把事业做大。
      第三，勇气--冒险的勇气、行动的勇气。人的自由在于人的主动性，人的主动性在于向多种可能性敞开。假如你不尝试什么，你就不会真正知道自己是什么，你也不会知道自己到底要什么。所以有这样一句名言倒是很值得一记：“举枪──射击──瞄准”。“毋惮初难，毋恃久安”，先做了再说。生机在行动中来，从动态发展中来。不要常常抱怨自己缺少机会，行动乃为机会之母。
      第四，坚持。许多事没有成功，不是由于构想不好，也不是由于没有努力，而是由于努力不够。时下有个概念叫开拓者。什么叫开拓？开拓就是除了开辟还需拓进。坚持就是拓进，就是遇到困难也绝不放弃的韧劲。
      就这样，目标、胸怀、勇气、坚持，加上聪明，构成了成功者最为重要的五要素。但是，聪明只占第五位。由于我们相信人的聪明属于天生的并不多，它更多的成分来源于后天的造就，所以，假如有了前四者，聪明也会更容易产生。
      正如大多数成功者所证明，成功最主要的因素就是心智开发。总结上述五点，前四者所说全是“心”，最后一者所说才是“智”。也就是说，对成功者来说，发展自己的各种智力固然很重要，但更主要的，还是你对“心”把握。
      于是，如何把心性练大，把心力练强，这就是追求成功者最需要关心的问题。
      
【温馨提示】

      世间没有绝对的标准。人们通常所谓的标准，都是人为制定的，它会随着时间以及社会的发展有所改变。千万不要用一把绝对的尺子作为衡量的标准。在学习上敢于质疑，敢于突破，才能取得更大的进步。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "船王哈特",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      英国船王老哈特，在选择儿子小哈特能否接班时，表现得犹豫不决，他拿不准儿子是否能担起船王的重任。于是，他开始向世人征求意见，发布消息：凡是了解儿子的人都可以提出自己的意见，看看小哈特的身上都有什么缺陷，尤其是那些致命的缺陷。
      老哈特的这个信息发布以后，在社会上引起了不小的轰动。于是，小哈特身上的弱点和不足很快便被一一找了出来：首先小哈特的成长经历不够丰富，他几乎没有经过什么风浪，社会经验比起老哈特，更是相差甚远。何况他还不善于管理……意见一一击中了小哈特的要害，令老哈特更加忧郁。
      一天，老哈特去教堂时，把自己的苦恼讲给了牧师。牧师听后笑了起来，说错误根本不在小哈特，而是在老哈特的身上。老哈特听了瞠目结舌……回去之后，老哈特按照牧师的方法，重新发布消息，让大家看看小哈特身上到底有什么优点。第二天，就有人总结出了一堆小哈特的优点，他聪明，善于及时发现问题，做事肯下工夫，而且很容易与人打成一片……几天过后，小哈特的全身都成了优点。
      不久，小哈特继承了老船王的工作，并很快使哈特家族的事业有了耀眼之处。
      
【温馨提示】

      世上的许多决定，原本都在我们最初的决定中。所谓的选择，很多时候，都是被我们先入为主的担心所左右着。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "龙虾与寄居蟹",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一天，龙虾与寄居蟹在深海中相遇，寄居蟹看见龙虾正把自己的硬壳脱掉，只露出娇嫩的身躯。寄居蟹非常紧张地说：“龙虾，你怎可以把唯一保护自己身躯的硬壳也放弃呢？难道你不怕有大鱼一口把你吃掉吗？以你现在的情况来看，连急流也会把你冲到岩石去,到时你不死才怪呢？”龙虾气定神闲地回答：“谢谢你的关心，但是你不了解，我们龙虾每次成长，都必须先脱掉旧壳，才能生长出更坚固的外壳，现在面对的危险，只是为了将来发展得更好而做出准备。”寄居蟹细心思量一下，自己整天只找可以避居的地方，而没有想过如何令自己成长得更强壮，整天只活在别人的护阴之下，难怪永远都限制自己的发展.。
      
【温馨提示】

      对于那些害怕危险的人，危险无处不在。每个人都有一定的安全区，你想跨越自己过去的成绩，就不要划地自限，勇于接受挑战充实自我，你一定会发展得比想象中更好。现在刚进入预初的孩子何尝不是这样，小学时的优秀已成为过去，在中学就应该学会挑战自我－自主学习，不要在家长的护荫之下被动学习了，告诉世界我能行！",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "三个小金人的故事",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      曾经有个小国给中国进贡了三个一模一样的小金人，皇帝高兴坏了，可是这小国的使者不厚道，同时出一道题目：这三个金人哪个最有价值？皇帝想了许多的办法，请来珠宝匠检查，称重量看做工，都是一模一样的，怎么办？使者还等着回去汇报呢。泱泱大国，不会连这个事都不懂吧？
      最后，有一位退位的老臣来回答了使者的问题。老臣胸有成竹地拿着三根稻草，插入第一个金人的耳朵里，这稻草从另一边耳朵出来了。第二个金人的稻草从嘴巴里直接掉出来，而第三个金人，稻草进去后掉进了肚子，什么响动也没有。老臣说：第三个金人最有价值！使者默默无语，答案正确。
      
【温馨提示】

      这个故事告诉我们，有价值的人，不一定是最能说的人的人。老天给我们两只耳朵一个嘴巴，本来就是让我们多听少说的。善于倾听，才是最基本的素质。课堂上认真倾听老师的讲课，哪怕是自己已经知晓的知识，也应该保持安静，既尊重老师，又尊重正在听课的同学，更是对自己尊重。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "认真的力量",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1944年冬天，盟军完成了对德国的铁壁合围，第三帝国覆亡在即。整个德国笼罩在一片末日的氛围里，经济崩溃，物资奇缺，老百姓的生活很快陷入严重困境。对普通平民来说，食品短缺就已经是人命关天的事。更糟糕的是，由于德国地处欧洲中部，冬季非常寒冷，家里如果没有足够的燃料，根本无法挨过漫长的冬天。在这种情况下，各地政府只得允许让老百姓上山砍树。你能想象帝国崩溃前夕的德国人是如何砍树的吗？在生命受到威胁时，人们非但没有去哄抢，而是先由政府部门的林业人员在林海雪原里拉网式地搜索，找到老弱病残的劣质树木，做上记号，再告诫民众：如果砍伐没有做记号的树，将要受到处罚。在有些人看来，这样的规定简直就是个笑话：国家都快要灭亡了，谁来执行处罚？然而令人不可思议的是，直到第二次世界大战彻底结束，全德国竟然没有发生过一起居民违章砍伐无记号树木的事，每一个德国人都忠实地执行了这个没有任何强制约束力的规定。
      这是著名学者季羡林先生在回忆录《留德十年》里讲的一个故事。当时她在德国留学，目睹了这一幕，所以事隔50多年，他仍对此事感叹不已，说德国人“具备了无政府的条件，却没有无政府的现象”。
      是一种什么样的力量使得德国人在如此极端糟糕的情况下，仍能表现出超出一般人想象的自律？答案只有两个字：认真。因为认真是一种习惯，它深入到一个人的骨髓中，融化到一个人的血液里。因了这两个字，德意志民族在经历了上个世纪初、中叶两次毁灭性的世界大战之后，又奇迹般地迅速崛起。如果说强大的德意志是一个可怕的民族，那么，认真也是一种可怕的力量，它大能使一个国家强盛，小能使一个人无往而不胜。
      
【温馨提示】

      一旦“认真”二字深入到自己的骨髓，融化进自己的血液，你也会焕发出一种令所有的人、包括自己都感到害怕的力量。这里讲的认真并不是顽固不化、钻牛角尖，而是作为学生应该有认真的学习态度，不要因为自己学过了就不认真听讲了，不要因为一次考试成绩较满意就忽视今后的课前预习、课后复习，知识有一个积累的过程，基础不牢固何以建高楼。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "条件反射与习惯",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      父子两住山上，每天都要赶牛车下山卖柴。老父较有经验，坐镇驾车，山路崎岖，弯道特多，儿子眼神较好，总是在要转弯时提醒道：“爹，转弯啦！”有一次父亲因病没有下山，儿子一人驾车。到了弯道，牛怎么也不肯转弯，儿子用尽各种方法，下车又推又拉，用青草诱之，牛一动不动。到底是怎么回事？儿子百思不得其解。最后只有一个办法了，他左右看看无人，贴近牛的耳朵大声叫道：“爹，转弯啦！”牛应声而动。
      
【温馨提示】

      牛用条件反射的方式活着，而人则以习惯生活。一个成功的人晓得如何培养好的习惯来代替坏的习惯，当好的习惯积累多了，自然会有一个好的人生。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "意志力的较量",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      笑话中的哲理：
      王老五从来没有出过远门，攒了半辈子的钱，终于参加一个旅游团出了国。国外的一切都是非常新鲜的，关键是，王老五参加的是豪华团，一个人住一个标准间。这让他新奇不已。
      早晨，服务生来敲门送早餐时大声说道：“goodmorningsir！”王老五愣住了。这是什么意思呢？在自己的家乡，一般陌生的人见面都会问：“您贵姓？”于是王老五大声叫道：“我叫王老五！”如是这般，连着三天，都是那个服务生来敲门，每天都大声说：“goodmorningsir！”而王老五亦大声回道：“我叫王老五！”但他非常的生气。这个服务生也太笨了，天天问自己叫什么，告诉他又记不住，很烦的。终于他忍不住去问导游，“goodmorningsir！”是什么意思，导游告诉了他，天啊！！真是丢脸死了。王老五反复练习“goodmorningsir！”这个词，以便能体面地应对服务生。又一天的早晨，服务生照常来敲门，门一开王老五就大声叫道：“goodmorningsir！”与此同时，服务生叫的是：“我是王老五！”
      
【温馨提示】

      这个故事看似笑话，其实要告诉我们:人与人交往，常常是意志力与意志力的较量。不是你影响他，就是他影响你。而我们要想成功，一定要培养自己的影响力，只有影响力大的人才可以成为最强者。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "骆驼妈妈和骆驼宝宝",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在动物园里的骆驼宝宝问妈妈：“妈妈，妈妈，为什么我们的睫毛那么地长？”
      骆驼妈妈说：“当风沙来的时候，长长的睫毛可以让我们在风暴中都能看得到方向。”
      骆驼宝宝又问：“妈妈妈妈，为什么我们的背那么驼，丑死了！”
      骆驼妈妈说：“这个叫驼峰，可以帮我们储存大量的水和养分，让我们能在沙漠里耐受十几天的无水无食条件。”
      骆驼宝宝又问：“妈妈妈妈，为什么我们的脚掌那么厚？”
      骆驼妈妈说：“那可以让我们重重的身子不至于陷在软软的沙子里，便于长途跋涉啊！”
      骆驼宝宝高兴坏了：“哗，原来我们这么有用啊！！可是妈妈，为什么我们还在动物园里，不去沙漠远足呢？”
      
【温馨提示】

      世界有多大，机会就有多大。机会有多大，少年的天地就有多大。每人的潜能是无限的，关键是要找到一个能充分发挥潜能的舞台。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "不同习惯的结果",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一根小小的柱子，一截细细的链子，拴得住一头千斤重的大象，这不荒谬吗？可这荒谬的场景在印度和泰国随处可见。当大象还是小象的时候，驯象人就用一条铁链将它绑在水泥柱或钢柱上，无论小象怎么挣扎都无法挣脱。小象渐渐地习惯了不挣扎，直到长成了大象，可以轻而易举地挣脱链子时，也不挣扎。
      驯虎人本来也像驯象人一样成功，他让小虎从小吃素，直到小虎长大。老虎不知肉味，自然不会伤人。驯虎人的致命错误在于他摔跤之后让老虎舔净他流在地上的血，老虎一舔不可收，终于将驯虎人吃了。
      小象是被链子绑住，而大象则是被习惯绑住。老虎曾经被习惯绑住，而驯虎人则死于习惯（他已经习惯于他的老虎不吃人）。
      
【温馨提示】

      习惯几乎可以绑住一切，只是不能绑住偶然，比如那只偶然尝了鲜血的老虎。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "蜜蜂与蝴蝶(一)",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一只蜜蜂和一只蝴蝶落在同一花枝上，蜜蜂问蝴蝶：“你们是干什么来的？”
      “我们是来玩的。”蝴蝶回答道，并且接着反问蜜蜂：“你们是干什么来的？”
      “我们是来采蜜的。”
      “采蜜的？”
      “嗯，采蜜的。”
      “采蜜做什么？”
      “供人享用。”
      “什么是蜜？”
      “蜜就是花的精髓。”
      “我也生活在鲜花丛中，怎么没有见过蜜呢？”
      “要得到蜜，需要有发现的眼睛和辛勤的劳动。”
      “蜜是什么样的？”
      “金黄色的，非常甜美，人们常用它来比喻美好的生活和爱情。”
      “这么说你们对人类的贡献可大啦。”
      “是呀是呀，所以我们没有那么多的闲时间来玩。”
      “那么，我们蝴蝶对人有什么用处呢？”
      “你们呀，一无是处！”
      它们的对话刚进行到这里，有一群割草的孩子突然来了。孩子们看到红的、白的、黄的、蓝的、黑的、花的蝴蝶在四处翩翩起舞，高兴得狂呼乱叫。——蝴蝶们那轻盈洒脱，千姿百态，来去自如，闪烁不定的美丽身影牢牢地吸引住了他们的视线，使他们几乎达到忘乎所以的程度，追着蝴蝶到处乱跑，开心得如同一群天使一般。这时，那只蜜蜂和那只蝴蝶又开始说话了：
      “怪，人们好像更喜欢你们。”蜜蜂惊讶地说。
      “嘿嘿，看来我们并不是一无是处。”蝴蝶有点讽刺意味地道。
      “难道你们生来是供人们观赏的？”
      “是的，你们为人类贡献的是蜜，我们贡献的是美；蜜能满足人的物质需要，美能满足人的精神需要，所以我们和你们对人来说都是有用处的，只不过是用处不同而已。”
      “这，我懂了，我懂了。”蜜蜂连连点头道。
      
【温馨提示】

      没有谁生来就是一无是处，善于发现自己优点并充分发挥优势，相信自己“我能行”。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "兴趣是最好的老师",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      顺着山谷去爬一座白云缭绕的大山，没想到不少人还带着七八岁的孩子。登山，大人都爬着费劲，何况孩子。可是孩子们欢呼着，兴奋着，叽叽喳喳，总是走在大人的前面。山谷蔚秀，岚云清流，野花杂树，芳草萋萋，石阶深深，引得孩子们欢呼雀跃。到了山上，还要走十多里山路去一座寺庙，孩子们一听更是兴致勃勃，走在前面开路。一天下来，走了四十里路，大人们个个累得腰酸腿疼、东倒西歪，孩子们却兴奋地仰着小脸问：“什么时候还来这里玩呢？”大人们纳闷：平常在城里走路，孩子们走几里就嚷着腿疼，爬山走这么远的路，孩子们怎么都不嚷累呢？
      鸟语花香的美景，将孩子们引入幽境；目不暇接的新奇，让他们兴奋；柳暗花明的境遇，充满悬念和爽心。
      
【温馨提示】

      兴趣是生命飞翔的翅膀，它能将枯燥和沉闷转化为鲜活生动和美丽轻盈。兴趣是最好的老师，它能激发你的潜能，开发你的智慧，磨练你的意志，陶冶你的性情，使你克服重重困难，攀上成功的顶峰。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "老鹰再生",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      老鹰是世界上寿命最长的鸟类。它一生的年龄可达70岁。要活那么长的寿命，它在40岁时必须做出困难却重要的决定。当老鹰活到40岁时，它的爪子开始老化，无法有效地抓住猎物。它的啄变得又长又弯，几乎碰到胸膛。它的翅膀变得十分沉重，因为它的羽毛长得又浓又厚。使得飞翔十分吃力。它只有两种选择：等死或经过一个十分痛苦的更新过程——150天漫长的操练。它必须很努力地飞到山顶。在悬崖上筑巢。停留在那里，不得飞翔。老鹰首先用它的喙击打岩石，直到完全脱落。然后静静地等候新的喙长出来。它会用新长出的喙把指甲一根一根的拔出来。当新的指甲长出来后，它们便把羽毛一根一根的拔掉。5个月以后，新的羽毛长出来了，再生的老鹰开始飞翔。
      
【温馨提示】

      改变自己需要勇气和决心。不好的习惯也是长时间形成的，要改变它并不意味着你一两天的不同行为就能改变，好习惯的养成是日积月累的结果，不要欲速而不达，更不能讳疾忌医，不能对自己的缺点过分宽容，要敢于面对，并要有改掉缺点的决心，这样我们才能进步，才能有更大有成就。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "牵蜗牛散步",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      上帝给一个小伙子一项任务，叫他牵一只蜗牛去散步。
      小伙子不能走得太快，蜗牛已经尽力爬，每次总是挪那么一点点。他催促蜗牛，吓唬它，责备它。蜗牛用抱歉的眼光看着他，说：“我已经尽了全力，总要一步一步走啊。”小伙子拉蜗牛，扯它，他甚至想踢蜗牛。蜗牛受了伤，它流着汗，喘着气，还是一步步往前爬。“真奇怪，为什么上帝叫我牵一只蜗牛去散步？”他想：“上帝啊！为什么？”天上一片安静。“唉！也许上帝去抓蜗牛了吧？好吧，松手吧！反正上帝不管了，我还管什么？蜗牛愿意怎么走就怎么走吧。”小伙子任蜗牛往前爬，他在后面生闷气。咦？他闻到花香，原来这路边有个花园。他听到鸟声，他听到鸟鸣，他看到满天的美丽的星斗。咦？以前怎么没有这种体会？莫非是他弄错了？小伙子忽然明白，上帝叫他牵蜗牛散步的原因了。
      
【温馨提示】

      人应珍惜每一天里每一步的价值。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "相信自己就是你迈开成功的第一步",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有个东方人曾经遍游世界寻找最聪明的人。他听人说世界上最聪明的人住在高高的喜玛拉雅山的山洞里。于是他收拾行装出发。穿过群山和沙漠，几个月后他来到了一座高山脚下。他骑着马走上窄窄的山间小道，到了一个小山洞前。
      “你是因智慧而扬名天下的最聪明的人吧?”他问坐在山洞里的老人。老人站起来．走到光亮的露天，看着这位旅行者的脸说：“是的．我是因为聪明而出名，你有什么问题吗？”“智慧老人．我怎样才能变得伟大？我上那儿才能找到智慧？”智慧老人盯着旅行者焦急的眼睛看了一会儿回答道：“你在哪儿能找到你的马?”说完他转身回到山洞中去。
      
【温馨提示】

      旅行者的马一直跟着他，伟大和智慧也一直跟着他，人们往往自问：“我行吗?”其实答案就在您的脚下——相信自己，就是你迈开成功的第一步。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "学会抓重点",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一位表演大师上场前，他的弟子告诉他鞋带松了。大师点头致谢，蹲下来仔细系好。等到弟子转身后，又蹲下来将鞋带解松。有个旁观者看到了这一切，不解地问：“大师，您为什么又要将鞋带解松呢？”大师回答道：“因为我饰演的是一位劳累的旅者，长途跋涉让他的鞋事松开，可以通过这个细节表现他的劳累憔悴。”“那你为什么不直接告诉你的弟子呢？”“他能细心地发现我的鞋带松了，并且热心地告诉我，我一定要保护他这种热情的积极性，及时地给他鼓励，至于为什么要将鞋带解开，将来会有更多的机会教他表演，可以下一次再说啊。”
      
【温馨提示】

      人一个时间只能做一件事，懂抓重点，才是真正的人才。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "沙砾与黄金",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一队商人骑着骆驼在沙漠里行走，突然空中传来一个神秘的声音：“抓一把沙砾放在口袋里吧，它会成为金子。”有人听了不屑一顾，根本不信，有人将信将疑，抓了一把放在袋里。有人全信尽可能地抓了一把又一把沙砾放在大袋里，他们继续上路，没带沙砾的走得很轻松，而带了沙砾的走得很沉重。很多天过去了，他们走出了沙漠，抓了沙砾的人打开口袋欣喜地发现那些粗糙沉重的沙砾都变成了黄灿灿的金子。
      
【温馨提示】

      在每个人漫长的一生中，时间就像是地上的沙砾，惟有紧紧抓住了时间的人，才能将这些普通粗糙的沙砾变成可贵的金子。不紧紧抓住时间的人固然轻松潇洒，但是他的生命长河里总是如沙一样黯淡粗糙，发不出如金子般灿烂的光辉。问问自己，今天抓了多少沙砾在口袋里？",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "动物园里的袋鼠笼",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一天动物园管理员发现袋鼠从笼子里跑出来了，于是开会讨论，一致认为是笼子的高度过低。所以他们决定将笼子的高度由原来的10米加高到20米。结果第二天他们发现袋鼠还是跑到外面来，所以他们又决定再将高度加高到30米。没想到隔天居然又看到袋鼠全跑到外面，于是管理员们大为紧张，决定一不做二不休，将笼子的高度加高到100米。
      一天长颈鹿和几只袋鼠们在闲聊，“你们看，这些人会不会再继续加高你们的笼子？”长颈鹿问。“很难说。”袋鼠说∶“如果他们再继续忘记关门的话！”
      
【温馨提示】

      事有“本末”、“轻重”、“缓急”，关门是本，加高笼子是末，舍本而逐末，当然就不得要领了。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "乌鸦搬家",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一天，有只乌鸦向东方飞去。在途中，它遇到一只鸽子，大家停下来休息。鸽子非常关心地问乌鸦：“乌鸦，你要飞到哪里去呀?”乌鸦愤愤不平地回答：“鸽子老弟，这个地方的人都嫌我的声音难听，所以我想飞到别的地方去。”鸽子听后，赶快忠告乌鸦说：“乌鸦老兄，你飞到别的地方还是一样有人讨厌你的，你自己若不改变声音和形象，到哪里都没有人欢迎您的。”乌鸦听了，惭愧地低下了头。
      
【温馨提示】

      许多人总喜欢责怪别人，怪环境不好，怪别人不喜欢他不欢迎他，但他总不反省自己的为人举止，是否值得他人尊重及欢迎。假如一个人不经常反省自己，只会责怪别人和环境，他就会和这只乌鸦一样，到处惹人讨厌。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "行动比知识重要",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      螃蟹、猫头鹰和蝙蝠去上恶习补习班。数年过后，它们都顺利毕业并获得博士学位。不过，螃蟹仍横行，猫头鹰仍白天睡觉晚上活动，蝙蝠仍倒悬。这是黄永玉大师的一个寓言故事，它的寓意很简单：行动比知识重要。
      学过的知识如果没有身体力行，那样知识就只是遥远的知识，知识并没有化成他们自己的生命体验。学到一个好知识，就立即在自己的生命中去执行。这样一来，那些遥远的知识就变成了真切的生命体验，他不必“懂”太多，就可以帮助自己，并帮助很多人。
      高敏感度是一种天才素质，那么高行动力是更重要的天才素质。
      最重要的力量永远在你自己的身上，奥秘的知识、玄妙的潜能开发、炫目的成功学等等，都远不如你自己身上已有的力量重要。我们习惯去外面寻找答案，去别人那里寻找力量，结果忘记了力量就在自己身上。别人的知识不能自动地拯救你。如果一些连珠的妙语打动了你，如果一些文字或新信条启发了你。那么，这些别人的文字和经验都只是一个开始，更重要的是，你把你认为好的知识真正运用到你自己的生命中去。
      犹太哲学家马丁?布伯说过：你必须自己开始。假如你自己不以积极的爱去深入生存，假如你不以自己的方式去为自己揭示生存的意义，那么对你来说，生存就将依然是没有意义的。
      
【温馨提示】

      许多人总喜欢责怪别人，怪环境不好，怪别人不喜欢他不欢迎他，但他总不反省自己的为人举止，是否值得他人尊重及欢迎。假如一个人不经常反省自己，只会责怪别人和环境，他就会和这只乌鸦一样，到处惹人讨厌。",
            group7));

            group7.Items.Add(new SampleDataItem("Group-4-Item-3",
            "每天做好一件事",
            "Item Subtitle: 3",
            "Assets/title/7.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一位画家，举办过十几次个人画展。开始无论参观者多少，脸上总是挂着微笑。有一次，我问他：'你为什么每天都这么开心呢？'他给我讲一件事情：小时后，我兴趣非常广泛，也很要强。画画、弹钢琴、游泳、打篮球，必须都得第一才行。这当然是不可能的。于是，我心灰意冷，学习成绩一落千丈。父亲知道后，找来一个漏斗和一捧玉米种子。让我双手放在漏斗下面接着，然后捡起一粒种子投到漏斗里面，种子便顺着漏斗滑到了我的手里。父亲投了十几次，我的手中也就有了十几粒种子。然后，父亲一次抓起满满的一把玉米粒放在漏斗里面，玉米粒相互挤着，竟一粒也没有掉下来。父亲对我说：'这个漏斗代表你，假如你每天都能做好一件事，每天你就会有一粒种子的收获和快乐。可是，当你想把所有的事情都挤到一起来做，反而连一粒种子也收获不到了。'
      
【温馨提示】

      许多人总喜欢责怪别人，怪环境不好，怪别人不喜欢他不欢迎他，但他总不反省自己的为人举止，是否值得他人尊重及欢迎。假如一个人不经常反省自己，只会责怪别人和环境，他就会和这只乌鸦一样，到处惹人讨厌。",
            group7));



            this.AllGroups.Add(group7);
            #endregion

            #region 第8章
            var group8 = new SampleDataGroup("Group-8",
            "第8章",
            "Group Subtitle: 3",
            "Assets/title/8.png",
            "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
"把缺点转化成发展自己的机会",
"Item Subtitle: 3",
"Assets/title/8.png",
"Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
@"
      曾长期担任菲律宾外长的罗慕洛穿上鞋时身高只有1.63米。原先，他与其他人一样，为自己的身材而自惭形秽。年轻时，也穿过高跟鞋，但这种方法终令他不舒服，精神上的不舒服。他感到自欺欺人，于是便把它扔了。后来，在他的一生中，他的许多成就却与他的“矮”有关，也就是说，“矮”倒促使他成功。以至他说出这样的话：“但愿我生生世世都做矮子。”
      1935年，大多数的美国人尚不知道罗慕洛为何许人也。那时，他应邀到圣母大学接受荣誉学位，并且发表演讲。那天，高大的罗斯福总统也是演讲人，事后，他笑吟吟地怪罗慕洛“抢了美国总统的风头”。更值得回味的是，1945年，联合国创立会议在旧金山举行。罗慕洛以无足轻重的菲律宾代表团团长身份，应邀发表演说。讲台差不多和他一般高。等大家静下来，罗慕洛庄严地说出一句：“我们就把这个会场当作最后的战场吧。”这时，全场登时寂然，接着爆发出一阵掌声。最后，他以“维护尊严、言辞和思想比枪炮更有力量……唯一牢不可破的防线是互助互谅的防线”结束演讲时，全场响起了暴风雨般的掌声。后来，他分析道：如果大个子说这番话，听众可能客客气气地鼓一下掌，但菲律宾那时离独立还有一年，自己又是矮子，由他来说，就有意想不到的效果，从那天起，小小的菲律宾在联合国中就被各国当作资格十足的国家了。
      由这件事，罗慕洛认为矮子比高个子有着天赋的优势。矮子起初总被人轻视，后来，有了表现，别人就觉得出乎意料，不由得佩服起来，在人们的心目中，成就就格外出色，以致平常的事一经他手，就似乎成了破石惊天之举。
      
【温馨提示】

      ",
group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "46厘米的骑士",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位英国的成年人在洗脸的时候，意外遇到了他一生中最危险的事情。什么事情？他差一点被淹死——淹死在精致的脸盆里！这是天方夜谭吗？不是。如果不是，那你肯定会异常惊讶。但是，如果你知道了他的身高就不会惊讶了，因为他只有46厘米高。这位身高如同婴儿的成年人在现实生活中会遭遇到什么呢？找不到工作，娶不上媳妇，还是会遭人歧视？如果你这样认为，那就大错特错了。让我们先简要说说他的生平吧！
      他从小就受到英国王室的青睐，9岁进入白金汉宫，不久，进入英国王宫，效力于国王和王后，他的第一个皇室职位是汉普顿宫廷委员会委员。11岁时，他进入英国外交部。他第一次出访的目的地是法国，返航途中，船队在敦刻尔克被海盗劫持。整个反劫持过程中，他沉着机智，大义凛然，为全体出访人员安全脱身立下了汗马功劳。还在十几岁时，他便跟随英国的将军前往荷兰，支援荷兰反抗西班牙、争取独立的斗争。经过拼搏，他立下了赫赫战功。19岁的他，成为家喻户晓的英雄。回国后，他被封为爵士。到了61岁，他还不甘寂寞，加入国王的秘密军队……
      翻开该国的历史，我们就会知道，这位传奇人物就是响当当的赫德森。在17世纪的绘画和文学作品中，他的身影无处不在。对他来说，找不到工作，是空穴来风；娶不上媳妇，是无稽之谈；遭人歧视，更是天方夜谭。赫德森，是无可争议的英王查理一世手下最有影响力的骑士。就是这位显赫的赫德森爵士，没有被战场上的枪林弹雨击倒，没有被官场上的明枪暗箭射倒，没有被妒火中烧的情敌斗倒，反而差一点掉进脸盆里爬不出来！这两个极端，都出奇地汇聚在矮小的赫德森身上。惊世骇俗的英雄赫德森绝不是个传说，在现今英国牛津市的阿莫什林博物馆里，赫德森生前的衣物还陈列在那儿，其大小如同婴儿服装，这足以证明300年前的那个历史曾经真实的存在。
      看完这个历史人物的巨大反差，读者诸君有何感想？
      
【温馨提示】

      绝不要自己瞧不起自己，也绝不要轻易瞧不起别人。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "国王的梦",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      从前，有一位国王，梦见山倒了，水枯了，花也谢了，便叫王后给他解梦。王后说：“大势不好，山倒了指江山要倒；水枯了指民众离心，君是舟，民是水，水枯了，舟也不能行了；花谢了指好景不长了。”国王惊出一身冷汗，从此患病，且愈来愈重。一位大臣参见国王，国王在病榻上说出了他的心事，大臣一听，笑着说：“这是好兆头，山倒了指从此天下太平；水枯指真龙现身，国王，你是真龙天子啊；花谢了，花谢见果子呀！”国王顿觉全身轻松，很快痊愈。
      王后和大臣不同的说法，也让国王有了截然不同的境遇。可见，消极的人看到的是困扰，让人感受到生活处处是阴影；积极的人看到的是希望，让心灵拥有更加广阔的晴空。
      拿破仑曾说：“人与人之间只有很小的差异，但是这种很小的差异却可以造成巨大的差异。很小的差异即积极的心态还是消极的心态，巨大的差异就是成功和失败。”
      
【温馨提示】

      成功和失败之间的区别在于心态的差异：即成功者着意亮化积极的一面，失败者总是沉迷消极的一面。任何人的生活，都会有为外界干扰、为旁人左右的时候。外来的因素总是起着或积极或消极的作用。所以，作为一个正常的人，在遇到生活的难题时，重要的是不管别人的心态如何，都要有自己正确的立场，保持良好的心态，否则一不小心，就会偏离正常的生活轨道。任何情况下，积极的心态有助于人们克服困难，看到希望，保持旺盛斗志；而消极的心态只会使人沮丧失望，对生活充满抱怨，甚至限制和扼杀自身的潜力。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "给失败一个心理准备",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      心理学家们曾做过这样一个实验：在给小小的缝衣针穿线的时候，你越是全神贯注地努力，线越不容易穿入。在科学界，这种现象被称为“目的颤抖”，目的性越强就越不容易成功。
      这种现象在生活中并不鲜见。
      张师傅是一名杂技演员，脚耍大缸已有多年，可谓驾轻就熟。因为年龄偏大，他决定改行。在告别舞台演出的那天晚上，他把亲戚、朋友都请来观看。然而，正当人们为他精湛的技艺喝彩时，他却“失手”了：因一脚顶偏，偌大的瓷缸重重地砸在他的鼻梁上，他当场昏了过去。事后有人问他：“凭你的技术，怎么会出此意外？”他说：“那天，心里总是想，这是自己杂技生涯的最后一场演出，而且请了那么多亲戚、朋友来捧场，一定要表演得很出色，千万不能出错。谁知表演时一走神儿，就出事了。”
      从表面上看，很多失手都是偶然的，其实却有其必然性。因为人有这样一个弱点：当对某件事情过于重视时，心理就会紧张；而一紧张，往往就会出现心跳加速、精力分散、动作失调等不良反应。很多人在人生的关口失手，心理紧张与焦虑是重要原因之一。
      
【温馨提示】

      做每一件事，我们都不能保证百分之百的成功。既然如此，我们何不给失败一个心理准备呢？成功之前先做好失败的准备，并非放弃对成功的追求，而是让我们放松心情，放下包袱，轻装上阵，如此一来反倒容易成功。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "信念就是种子",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一支英国探险队进入撒哈拉沙漠的某个地区，在茫茫的沙海里跋涉。阳光下，漫天飞舞的风沙像炒红的铁砂一般，扑打着探险队员的面孔。口渴似炙，心急如焚——大家的水都没了。这时，探险队长拿出一只水壶，说：“这里还有一壶水，但穿越沙漠前，谁也不能喝。”
      一壶水，成了穿越沙漠的信念之源，成了求生的寄托目标。水壶在队员手中传递，那沉甸甸的感觉使队员们濒临绝望的脸上，又露出坚定的神色。终于，探险队顽强地走出了沙漠，挣脱了死神之手。大家喜极而泣，用颤抖的手拧开那壶支撑他们的精神之水——缓缓流出来的，却是满满的一壶沙子！
      炎炎烈日下，茫茫沙漠里，真正救了他们的，又哪里是那一壶沙子呢？他们执着的信念，已经如同一粒种子，在他们心底生根发芽，最终领着他们走出了“绝境”。
      
【温馨提示】

      人生就是这样，只要种子还在，希望就在。人生从来没有真正的绝境，无论遭受多少艰辛，无论经历多少苦难，只要一个人的心中还怀着一粒信念的种子，那么总有一天，他就能走出困境，让生命重新开花结果。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "心灵先到达那个地方",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      美国西部的一个乡村，有一位清贫的少年,每当有了闲暇，他总要拿出祖父在他8岁那年送给他的生日礼物？那幅已被卷了边的世界地图看。15岁那年，这位少年写下了他的《一生的志愿》：“要到尼罗河、亚马逊河和刚果河探险；要登上珠穆朗玛峰、乞力马扎罗山和麦金俐峰；驾驭大象、骆驼、鸵鸟和野马；探访马可?波罗和亚历山大一世走过的道路；主演一部《人猿泰山》那样的电影；驾驶飞行器起飞降落；读完莎士比亚、柏拉图和亚里士多德的著作；谱一部乐曲；写一本书；拥有一项发明专利；给非洲的孩子筹集100万美元捐款……”他一口气列举了127项人生的宏伟志愿。毫无疑问，那是一场壮丽的人生跋涉。
      44年后，他终于实现了《一生的志愿》中的106个愿望……他就是上个世纪著名的探险家约翰?戈达德。当有人惊讶地追问他是凭借着怎样的力量，让他把那许多注定的“不可能”都踩在了脚下，他微笑着如此回答：“很简单，我只是让心灵先到达那个地方。随后，周身就有了一股神奇的力量。接下来，就只需沿着心灵的召唤前进好了”。
      
【温馨提示】

      “凡事预则立，不预则废。”——不管做什么事如果先有了打算，往往能取得好的效果，否则就有可能失败。生命的清单对每一个人都很重要，它可以让我们的人生变得积极主动起来。正如西方的一句谚语：“如果你不知道你要到哪儿去，那通常你哪儿也去不了。”青少年时期属于自己的人生才刚刚开始，正是需要规划的时候，清楚自己的个性、找出自己的特点、发现自己的兴趣、明确自己的优势、衡量自己与目标的差距，给自己的人生一个合适的定位，并且让自己在人生各个阶段都有一个清晰的目标，然后朝着各个阶段的目标努力，让自己的人生更为圆满。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "天堂和地狱的差别",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位行善的基督徙，临终后想见天堂与地狱究竟有何差异，于是天使就先带他到地狱去参观到了地狱，在他们面前出现一张很大的餐桌，桌上摆满了丰盛的佳肴。
      '地狱的生活看起来还不错嘛'
      '不用急，你再继续看下去。'
      过了一会，用餐的时间到了，只见一群瘦骨如柴的饿鬼鱼贯地入座，每个人手上拿着一双长十几尺的筷子，可是由于筷子实在是太长了，最后每个人都夹得到吃不到.
      '你真觉得很悲惨吗？我再带你到天堂看看。'
      到了天堂，同样的情景，同样的满桌佳肴，每个人同样用一双长十几尺的筷子，围着餐桌吃饭的可爱的人们.他们也同用同样的筷子夹菜，不同的是，他们围坐在对面的人吃菜，而对方也喂他吃，因此每个人都吃得很愉快。
      
【温馨提示】

      “凡事预则立，不预则废。”——不管做什么事如果先有了打算，往往能取得好的效果，否则就有可能失败。生命的清单对每一个人都很重要，它可以让我们的人生变得积极主动起来。正如西方的一句谚语：“如果你不知道你要到哪儿去，那通常你哪儿也去不了。”青少年时期属于自己的人生才刚刚开始，正是需要规划的时候，清楚自己的个性、找出自己的特点、发现自己的兴趣、明确自己的优势、衡量自己与目标的差距，给自己的人生一个合适的定位，并且让自己在人生各个阶段都有一个清晰的目标，然后朝着各个阶段的目标努力，让自己的人生更为圆满。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "树繁招风",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个夏秋相交的午后，我家门口的一棵柿子树被一场罕见的大风拦腰折断了。那棵树枝繁叶茂，果实累累，所以很是让人心痛。我问正在对树叹息的祖父：“冬天风也好大啊，还那么冷，它怎么就没被吹断呢？”祖父说：“这树毁就毁在枝叶太旺，招风啊！”随着经历渐多，我才慢慢体会到祖父话中的意味。柿子树在夏秋之际的断折，是因为它在最繁华的时节，背负了太多的沉重。而在深秋或寒冬，华叶落去，果实卸下，生命归于简单而平静，再面对风霜雪雨的袭击时，就显得无畏无惧，宁静泰然了。
      
【温馨提示】

      人生何尝不是如此？智慧，有时就是一种去繁就简的境界。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "人生路上取与舍",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      《淮南子?人间训》上“塞翁失马”故事相信大家已耳熟能详，人们用“塞翁失马，安知非福”来形容世事多变，得失无常有时坏事可以变成好事。
      人的一生好比走路，会遇到很多岔路口，每到一个路口都面临一次选择，而每次选择无不影响着未来。每一个人都会遇到这样那样的困难和挫折，是舍，是得？是放弃，是坚持？充满了辩证法。生活对人生最大的考验不仅是“得”，也有“失”，即放弃。哪些需要放弃，哪些永不放弃？此时此刻，需要智慧，也需要勇气。
      伏尔泰说：使人疲惫的不是远方的高山，而是鞋里的一粒沙子。在人生的道路上，我们必须学会随时倒出“鞋里”的那粒“沙子”。这小小的“沙粒”就是需要我们放弃的东西。什么也不放弃的人，往往会失去更珍贵的东西。
      放弃是一门学问，一种艺术，懂得放弃的人才会拥有更多。快乐的人放弃痛苦，高尚的人放弃庸俗，纯洁的人放弃污浊，善良的人放弃邪恶。聪明的人勇于放弃，高明的人乐于放弃，精明的人善于放弃。“舍清溪之幽，得江海之博”。经历风雨，未必能见到彩虹；但不经风雨，根本不可能见到彩虹。这或许就是人生的真谛。
      
【温馨提示】

      人生何尝不是如此？智慧，有时就是一种去繁就简的境界。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "学会善于用眼观察",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      早在500多年以前，有一位名叫科尔迪的阿拉伯牧羊人无意中发现，有一只山羊异常兴奋，在那蹦来跳去尽情撒欢。他感到非常奇怪，决心弄清楚原因何在，于是便开始留意观察。通过一连几天的仔细观察，他发现那只与众不同的山羊特别爱吃山坡一棵树上的红浆果，一吃就兴奋起来。他心怀探索的勇气，吃了那棵树上的一些红浆果，不一会儿的工夫，便体验到那种神情振奋的感觉，情不自禁地跳起了欢快的舞蹈。从那以后，科尔迪每次到山坡放牧，都要品尝红浆果。有那么一次，他在吃红浆果时，凑巧被一位路过的欧洲传教士瞧见了。科尔迪如实道来，传教士听后当即采摘了一些红浆果。他回到住所之后，将红浆果清洗几遍，用水煮出咖啡色。他耐心地品尝，最初的感觉有点苦，随之而来的是神清气爽，浑身都焕发出一种活力。从此，他每天都要煮一壶红浆果饮料滋润自己。经过传教士的热心宣传，周围的人也都如法炮制，一起分享着饮用后的振奋。咖啡的妙用得到初步验证之后，传教士又向欧洲商人做了介绍，立刻引起了他们的高度重视。他们将咖啡树移植到本土，大面积地推广，并引导人们消费。
      
【温馨提示】

      一个人能否有所发现的关键，并不在于自己眼睛的大小，而在于是否善于用自己的眼睛观察。对微小事物的仔细观察，是艺术、科学、事业和生命等获得成功的伴侣。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "皮鞋的由来",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      很久很久以前，人类都还赤着双脚走路。有一位国王到某个偏远的乡间旅行，因为路面崎岖不平，有很多碎石头，刺得他的脚又痛又麻。回到王宫后，他下了一道命令，要将国内的所有道路都铺上一层牛皮。他认为这样做，不只是为自己，还可造福他的人民，让大家走路时不再受刺痛之苦。但即使杀尽国内所有的牛，也筹措不到足够的皮革，而所花费的金钱、动用的人力，更不知几何。虽然根本做不到，甚至还相当愚蠢，但因为是国王的命令，大家也只能摇头叹息。一位聪明的仆人大胆向国王提出谏言：'国王啊！为什么您要劳师动众，牺牲那么多头牛，花费那么多金钱呢？您何不只用两小片牛皮包住您的脚呢？'国王听了很惊讶，但也当下领悟，于是立刻收回成命，采用这个建议。据说，这就是'皮鞋'的由来。
      
【温馨提示】

      想改变世界，很难；要改变自己，则较为容易。与其改变全世界，不如先改变自己--'将自己的双脚包起来'。改变自己的某些观念和作法，以抵御外来的侵袭。当自己改变后，眼中的世界自然也就跟着改变了。如果你希望看到世界改变，那么第一个必须改变的就是自己。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "小狮子学本领",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      森林之王狮子有一个宝贝儿子，转眼之间小狮子渐渐长大了。狮王想把小狮子培养成一个充满智慧的接班人，以使皇家的声望不受玷污。但他苦恼的是自己该请谁来培养小狮子。
      托付给狐狸吗？狐狸是聪明，然而他却老爱撒谎！跟撒谎的人打交道，那可要搞出乱子来的。或许鼹鼠合适些？经常听说鼹鼠的屋子里是井井有条的，没有勘探过的地方，他是决不走动一步，餐桌上摆的每一粒谷子，都是他亲自弄干净研磨过的。总而言之，据报告所说，鼹鼠是做小事情的大能手。且慢！据说，他对于近在眼前的事物是目光锐利的，然而他能看得到一码以外的事物吗？鼹鼠的制度是好的，但对咱们可不合适，谁都同意，狮子的王国是远胜于鼹鼠的，那么，为什么不让豹子来试试呢？豹子又勇敢又强壮，豹子能够整天给你讲战略。然而，政治——豹子就不太明白了，对于公民权利他一点儿都不懂。那么豹子在如何治理国家方面还能教些什么功课呢？一个国王必须是政治家，又是法官，还要是战士，而豹子真正擅长的就只有作战。国王的儿子是不宜接受豹子教育的。
      总而言之，狮王把所有的野兽都考虑过了，连在森林中名气大得像柏拉图在古代一样的大象，也被考虑过了。但在狮王看来，所有的野兽，都是聪明有限，愚笨透顶的。
      对他的邻国抱有深厚友谊的鹰王，知道了狮子大王的苦恼，就决意要为他的朋友大大的效劳。他要求亲自来教育小狮子。狮王高兴得几乎要跳起来，得到一位国王来做儿子的教师，可不是空前绝后的大造化吗？于是狮王高高兴兴地给儿子准备行装，让他出国留学。
      时间过得真快，一年、两年过去了，小狮子终于毕业了。在过去有那些日子里，听到老鹰给自己的汇报都是赞美之辞，所以他很高兴，准备开一个隆重的宴会欢迎儿子回来。同时狮子大王还把所有的野兽召来，让他们见识儿子学到的本领。
      那天，狮子大王拥抱了他的儿子，并且对他说道：“亲爱的儿子，当我归天的时候必须由你来治理国家，我一只脚已经踏在棺材里了，你还正是旭日东升呢。我极愿意让你接替我的王位，你先告诉我们，你学到了什么渊博的知识，你以为怎样才可以为你的子民谋得幸福。”“爸爸，”小狮子子答道，“多谢您给了我学习的机会，我已经学到了你们之中没有一个人能懂的学问：从老鹰到鹌鹑，各种各样的鸟，在什么地方能饲养得最好，他们最爱吃什么食物，他们的各种不同的样儿，各种鸟生的蛋，各种鸟所需的一切东西――我没有一件是不知道的。请您念念这张文凭上评语吧！因此，如果您的意思是叫我管理这个国家，我就要叫我的子民们立刻建筑鸟巢。”
      狮子大王仰天长叹，他的子民们也都流下了眼泪，议员们因为丢脸而低下头来。这是很明显的，年老的狮王懊悔也来不及了：他儿子的学业是徒劳无益的。
      
【温馨提示】

      学习应该有选择地学，是否适合自己，有了学习的重点，才能排除干扰，集中精力学好本领，而效率自然也高了。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "小时钟的故事",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一只新组装好的小钟放在了两只旧钟当中。两只旧钟“滴答”、“滴答”一分一秒地走着。其中一只旧钟对小钟说：“来吧，你也该工作了。可是我有点担心，你走完三千二百万次以后，恐怕便吃不消了。”“天哪！三千二百万次。”小钟吃惊不已。“要我做这么大的事？办不到，办不到。”另一只旧钟说：“别听他胡说八道。不用害怕，你只要每秒滴答摆一下就行了。”“天下哪有这样简单的事情。”小钟将信将疑。“如果这样，我就试试吧。”
      小钟很轻松地每秒钟“滴答”摆一下，不知不觉中，一年过去了，它摆了三千二百万次。
      
【温馨提示】

      学习虽然枯燥乏味，但点滴之水汇成江河，平凡小事成就伟大，不要太小瞧自己的能力！",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "高尔夫球的凹点",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      高尔夫球运动刚刚兴起时，有个奇怪现象，几乎所有的高尔夫球手都喜欢用旧球，特别是有划痕的球。原来，有划痕的球比光滑的新球有着更优秀的飞行能力。于是，根据空气动力学原理，科学家设计出了表面有凹点的高尔夫球。这些凹点，让高尔夫球的平稳性和距离性比光滑的球更有优势。从此，有凹点的高尔夫球成为比赛的统一用球。
      
【温馨提示】

      如果把高尔夫球的凹点比作人生的伤痕，那么，失败、坎坷和挫折这些给人带来伤痕的东西，并不可怕。因为，有些伤痕，也许会有利于让我们培养健康心理，从而有利于实现人生远大的目标。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "花瓶与木桶",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      如果花瓶碎了,怎么办？大多数人的做法是,把碎片扔掉!且一扔了事,干脆利索,全然不曾思考与之有关的规律。那么,这里头有规律吗？有。这就是,将碎片按大小排列并称过重量后即可发现:10～100克的最少,1～10克的稍多,0．1～1克的和0．1以下的最多!尤其有趣的是,这些碎片的重量之间有着严整的倍数关系,即:最大碎片与次大碎片的重量比为16:1,次大碎片与中等碎片的重量比为16:1,中等碎片与较小碎片的重量比是16:1,较小碎片与最小碎片的重量比也是16:1。于是,发现这一倍比关系的人便将此规律用于考古或天体研究,从而由已知文物、陨石的残肢碎片推测它的原状,并迅速恢复它们的原貌!这位极善思考的聪明人,就是丹麦科学家雅各布?博尔!可是,我们做到了吗？没有。打碎瓶子的经历,我们肯定有过,可是,当包含其间的规律从我们的身边淘气地溜走时,我们拥抱过它吗?没有！就因为迟钝!如此看来,花瓶碎了并不可怕,可怕的是:千万别一不留神,把我们的聪明打碎了!
      有位奥地利医生叫奥斯布鲁格,他父亲是个卖酒的,为了判明高大的酒桶里还有没有酒,这位父亲经常用手在桶外头敲敲,然后由声音判定桶里还有多少酒,是满桶还是空桶。父亲的这一做法启发了他,他便由此推论,人的胸腔腹腔不也像只桶吗?医生敲敲病人的胸腔腹腔并细心听听,不就可以由声音判明他的病情了吗?于是细细钻研,认真总结,终于发明了著名的诊病方法——叩诊。
      
【温馨提示】

      这个世界处处有哲学,瓶里有,桶里也有。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "拼地图游戏",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位牧师在一个星期六下雨的早晨，准备他的唠叨的讲道，可他的小儿子吵闹不休，令人讨厌，最后，这位牧师在失望中拾起一本旧杂志，一页一页地翻阅，直到翻到一幅色彩鲜艳的大图画———幅世界地图。他就从那本杂志上撕下这一页，再把它撕成了碎片，丢在起坐间的地上，说道：“小约翰，如果你能拼拢这些碎片，我就给你2角5分钱。”牧师以为这件事会使小约翰花费上午的大部分时间。但是没有10分钟，就有人敲他的房门。这是他的儿子。牧师惊愕地看到约翰如此之快地拼好了一幅世界地图。“孩子，你怎样把这件事做得这样快？”牧师问道，小约翰说：“这很容易。在地图的背面有一个人的照片。我就把这个人的照片拼到一起。然后把它翻过来。我想如果这个人是正确的，那么，这个世界就是正确的。”牧师微笑起来，给了他的儿子2角5分钱。“你替我准备好了明天的讲道。”
      
【温馨提示】

      如果一个人是正确的，他的世界也就会是正确的。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "锦鲤成长的故事",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      锦鲤俗称日本鲤鱼，是很多养鱼爱好者最爱的观赏鱼。日本鲤鱼的神奇之处在于，如果你在小鱼缸里饲养它，它只会长到两三寸长；如果你把它放入大鱼缸或者小池塘中，它就能长至六寸到一尺长；放进大一点的池塘，它能长到一尺半长；如果把它放进大湖之中，让它不受限制地充分成长，有朝一日它可能会长达三尺。日本鲤鱼能长到多大，与池塘的大小有直接关系。
      
【温馨提示】

      人的成长与日本鲤鱼非常相似。不过，对于人类而言，限制我们成长的不是外部世界，而是我们内心的世界。只有不断扩展我们的内心，我们才能不断的成长。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "好运气缘何降临七次?",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位孝顺的小男孩，实在心痛父母起早贪黑地工作却无法维持全家的温饱，所以偷偷溜到大街上想找个工作。他的运气还算不错，真的有一家商铺想招一个小店员。小男孩就跑去试。结果，跟他一样，共有七个小男孩都想在这里碰碰运气。店主说：“你们都非常棒，但遗憾的是我只能要你们其中的一个。我们不如来个小小的比赛，谁最终胜出了，谁就留下来。”这样的方式不但公平，而且有趣，小家伙们当然都同意。店主接着说：“我在这里立一根细钢管，在距钢管2米的地方画一条线，你们都站在线外面，然后用小玻璃球投掷钢管，每人十次机会，谁掷准的次数多，谁就胜了。”结果天黑前谁也没有掷准一次，店主只好决定明天继续比赛。
      第二天，只来了三个小男孩。店主说：“恭喜你们，你们已经成功地淘汰了四个竞争对手。现在比赛将在你们三个人中间进行，规则不变，祝你们好运。”前两个小男孩很快掷完了，其中一个还掷准了一次钢管。轮到这位有孝心的小男孩了。他不慌不忙走到线跟前，瞅准立在2米外的钢管，将玻璃球一颗一颗地投掷出去。他一共掷准了七下。
      店主和另两个小男孩十分惊诧：这种几乎完全靠运气的游戏，好运气为什么会一连在他头上降临七次？
      店主说：“恭喜你，小伙子，最后的胜者当然是你，可是你能告诉我，你胜出的诀窍是什么吗？”
      小男孩眨了眨眼睛说：“本来这比赛是完全靠运气的，不是吗？但为了赢得这运气，昨天我一晚上没睡觉，都在练习投掷。”
      
【温馨提示】

      人的成长与日本鲤鱼非常相似。不过，对于人类而言，限制我们成长的不是外部世界，而是我们内心的世界。只有不断扩展我们的内心，我们才能不断的成长。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "阴影是条纸龙",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      祖父用纸给孙子做过一条长龙.长龙腹腔的空隙仅仅只能容纳几只蝗虫,投放进去,它们都在里面死了,无一幸免!祖父说:'蝗虫性子燥,除了挣扎,它们没想过用嘴巴去咬破长龙,也不知道一直向前可以从另一端爬出来.因而,尽管它有铁钳般的嘴壳和锯齿一般的大腿,也无济于事.'当祖父把几只同样大小的青虫从龙头放进去,然后关上龙头,奇迹出现了:仅仅几分钟,小青虫们就一一地从龙尾爬了出来.
      
【温馨提示】

      命运一直藏匿在我们的思想里.许多人走不出人生各个不同阶段或大或小的阴影,并非因为他们天生的个人条件比别人要差多远,而是因为他们没有思想要将阴影纸龙咬破,也没有耐心慢慢地找准一个方向,一步步地向前,直到眼前出现新的洞天.阴影是条纸龙——人生中,经常有无数来自外部的打击,但这些打击究竟会对你产生怎样的影响,最终决定权在你手中.“荆棘不会来刺你，除非你靠着它。”",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "昂起头来真美",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      珍妮是个总爱低着头的小女孩,她一直觉得自己长得不够漂亮.有一天,她到饰物店去买了只绿色蝴蝶结,店主不断赞美她戴上蝴蝶结挺漂亮,珍妮虽不信,但是挺高兴,不由昂起了头,急于让大家看看,出门与人撞了一下都没在意.珍妮走进教室,迎面碰上了她的老师,'珍妮,你昂起头来真美!'老师爱抚地拍拍她的肩说.那一天,她得到了许多人的赞美.她想一定是蝴蝶结的功劳,可往镜前一照,头上根本就没有蝴蝶结,一定是出饰物店时与人一碰弄丢了.
      自信原本就是一种美丽,而很多人却因为太在意外表而失去很多快乐.
      
【温馨提示】

      无论是贫穷还是富有,无论是貌若天仙,还是相貌平平,只要你昂起头来,快乐会使你变得可爱——人人都喜欢的那种可爱。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "不要把自己当成老鼠",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1858年,瑞典的一个富豪人家生下了一个女儿.然而不久,孩子染患了一种无法解释的瘫痪症,丧失了走路的能力.
      一次,女孩和家人一起乘船旅行.船长的太太给孩子讲船长有一只天堂鸟,她被这只鸟的描述迷住了,极想亲自看一看.于是保姆把孩子留在甲板上,自己去找船长.孩子耐不住性子等待,她要求船上的服务生立即带她去看天堂鸟.那服务生并不知道她的腿不能走路,而只顾带着她一道去看那只美丽的小鸟.奇迹发生了,孩子因为过度地渴望,竟忘我地拉住服务生的手,慢慢地走了起来.从此,孩子的病便痊愈了.女孩子长大后,又忘我地投入到文学创作中,最后成为第一位荣获诺贝尔文学奖的女性,也就是茜尔玛-拉格萝芙.
      
【温馨提示】

      追求忘我——不要把自己当作老鼠,否则肯定被猫吃。忘我是走向成功的一条捷径,只有在这种环境中,人才会超越自身的束缚,释放出最大的能量.",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "人生其实可以很简单",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一个人去应征工作，随手将走廊上的纸屑捡起来，放进了垃圾桶，被路过的面试官看到了，因此他得到了这份工作。原来获得赏识很简单，养成好习惯就可以了。
      有个小弟在脚踏车店当学徒，有人送来一部故障的脚踏车，小弟除了将车修好，还把车子整理的漂亮如新，其他学徒笑他多此一举，后来雇主将脚踏车领回去的第二天，小弟被挖角到那位雇主的公司上班。原来出人头地很简单，吃点亏就可以了。
      有个小孩对母亲说：'妈妈你今天好漂亮。'母亲回答：'为什么？”小孩说：'因为妈妈今天都没有生气。“」原来要拥有漂亮很简单，只要不生气就可以了。
      有个牧场主人，叫他孩子每天在牧场上辛勤的工作，朋友对他说：“你不需要让孩子如此辛苦，农作物一样会长得很好的。”牧场主人回答说：“我不是培养农作物，我是在培养我的孩子。”原来培养孩子很简单，让他吃点苦头就可以了。
      有一个网球教练对学生说：“如果一个网球掉进草堆?应该如何找？”有人答：“「从草堆中心线开始找。”有人答：“从草堆的最凹处开始找。”有人答：“从草最长的地方开始找。”教练宣布正确答案：“按部就班的从草地的一头，搜寻到草地的另一头。”原来寻找成功的方法很简单，从一数到十不要跳过就可以了。
      有一家商店经常灯火通明，有人问：“你们店里到底是用什么牌子的灯管？那么耐用。”店家回答说：“我们的灯管也常常坏，是我们坏了就换而已。”原来保持明亮的方法很简单，只要常常更换就可以了。
      住在田边的青蛙对住在路边的青蛙说：“你这里太危险，搬来跟我住吧！”路边的青蛙说：“我已经习惯了，懒得搬了。”几天后，田边的青蛙去探望路边的青蛙，却发现他已被车子压死，暴尸在马路上。原来掌握命运的方法很简单，远离懒惰就可以了。
      有一只小鸡破壳而出的时候，刚好有只乌龟经过，从此以后小鸡就背着蛋壳过一生。原来脱离沉重的负荷很简单，放弃固执成见就可以了。
      有几个小孩很想当天使，上帝给他们一人一个烛台，叫他们要保持光亮，结果一天两天过去了，上帝都没来，所有小孩已不在擦拭那烛台，有一天上帝突然造访，每个人的烛台都蒙上厚厚的灰尘，只有一个小孩大家都叫他笨小孩，因为上帝没来，他也每天都擦拭，结果这个笨小孩成了天使。原来当天使很简单，只要实实在在去做就可以了。
      有只小狗，向神请求做他的门徒，神欣然答应，刚好有一头小牛由泥沼里爬出来，浑身都是泥泞，神对小狗说：“去帮他洗洗身子吧！”小狗讶异的答道：“我是神的门徒，怎么能去侍候那脏兮兮的小牛呢！”神说：“你不去侍候别人，别人怎会知道，你是我的门徒呢！”原来要变成神很简单，只要真心付出就可以了。
      有一支掏金队伍在沙漠中行走，大家都步伐沉重，痛苦不堪，只有一人快乐的走着，别人问：「你为何如此惬意？」他笑着：「因为我带的东西最少。」原来快乐很简单，拥有少一点就可以了。
      人生的光彩在哪里？
      早上醒来，光彩在脸上，充满笑容的迎接未来。
      到了中午，光彩在腰上，挺直腰杆的活在当下。
      到了晚上，光彩在脚上，脚踏实地的做好自己。
      原来人生也很简单，只要能懂得“珍惜、知足、感恩”你就拥有了生命的光彩。
      创业其实也很简单，一颗平常的心，一个好项目，一个努力奋进的人.....
      
【温馨提示】

      生活中的种种不顺心并非因为有多少重要的事没有做好，而仅仅是因为我们没去留意那些最简单的小事情。荆棘不会主动刺伤你，除非你主动靠近它！",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "人生秘诀",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      30年前，一个年轻人离开故乡，开始创造自己的前途。他动身的第一站，是去拜访本族的族长，请求指点。老族长正在练字，他听说本族有位后辈开始踏上人生的旅途，就写了3个字：不要怕。然后擡起头来，望着年轻人说：“孩子，人生的秘诀只有6个字，今天先告诉你3个，供你半生受用。”
      30年后，这个从前的年轻人已是人到中年，有了一些成就，也添了很多伤心事。归程漫漫，到了家乡，他又去拜访那位族长。他到了族长家，才知道老人家几年前已经去世，家人取出一个密封的信封对他说：“这是族长生前留给你的，他说有一天你会再来。”还乡的游子这才想起来，３０年前他在这听到人生的一半秘诀，拆开信封，面赫然又是3个大字：不要悔。
      
【温馨提示】

      中年以前不要怕，中年以后不要悔。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "微不足道的第一步",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      第二次世界大战结束后，一个年轻的奥地利大学生还沉浸在战争的恐怖中。他非常同情在战争中失去父母的孤儿，很想为那些可怜的孩子们做些什么。
      于是，他徒步从一个村庄走到另一个村庄，希望能够免费获得一块土地，为孤儿建造一个家。大学生不知走访了多少村落，终于在奥地利蒂罗尔州的一个小镇上，有好心人送给他一块荒芜的空地。大学生又马不停蹄地去寻找愿意捐赠建筑材料的慈善家和建筑工人。
      在他的不懈努力下，1949年，世界上第一个SOS儿童村在小镇因姆斯特建成，几十名孤儿在那里再一次感受到了家庭的温暖。这个大学生名叫赫尔曼?格迈纳尔，他就是SOS国际儿童村的创办人。今天，400多个SOS儿童村及其附属机构，分布在130多个国家和地区，抚养着无家可归的儿童，受益人超过30万。
      
【温馨提示】

      心中有爱成功就在眼前。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "司机考试的启示",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      某公司准备以高薪雇用一名小车司机，经过层层筛选和考试之后，只剩下三名技术最优良的竞争者。主考者问他们：“悬崖边有块金子，你们开着车去拿，觉得能距离悬崖多近而又不至于掉落呢?”
      “二公尺。”第一位说。
      “半公尺。”第二位很有把握地说。
      “我会尽量远离悬崖，愈远愈好。”第三位说。结果这家公司录取了第三位。
      
【温馨提示】

      不要和诱惑较劲，而应离得越远越好。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "成功有时需要“感谢”羞辱",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      美国国家安全顾问赖斯，10岁时全家到首都游览，却因身份是黑人，不能进入白宫。小赖斯备感羞辱，凝神远望白宫良久，然后回身告诉父亲：“总有一天，我会在那房子里！”果然，25年后，名牌大学毕业，已成为俄罗斯问题专家的赖斯，昂首阔步进入白宫担当了首席俄罗斯事务顾问，后又升为国家安全顾问，成为全世界著名外交家。白宫那条歧视黑人的规定，也早已烟消云散。
      美国NBA超级球星奥尼尔，当他还是一个高中生时，他的崇拜偶像是马刺队的中锋大卫?罗宾逊。在一次球赛后，苦苦等了几个小时的奥尼尔，看到偶像出来就兴冲冲地走上前去，请罗宾逊签字。可是罗连正眼都没看他，扬长而去。奥尼尔气得把签字本摔在地上，大吼一声：“你有什么了不起，我将来一定超过你！”五年后，NBA球场上出现了一个超级中锋，他就是“大鲨鱼”奥尼尔，在球场上见谁灭谁，所向无敌，尤其见了罗宾逊，更是发狠，每次都把罗宾逊打得丢盔卸甲。
      
【温馨提示】

      人无压力轻飘飘，不经激励不发愤。从这个意义上来说，我们确实应当“感谢”羞辱。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "沙子和珍珠",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      很久以前，有一个养蚌人，他想培育一颗世界上最大最美的珍珠。他去大海的沙滩上挑选沙粒，并且一颗一颗地问它们，愿不愿意变成珍珠。那些被问的沙粒，一颗一颗都摇头说不愿意。养蚌人从清晨问到黄昏，得到的还是同样的结果，他快要绝望了。就在这时，有一颗沙子答应了。因为，它一直想成为一颗珍珠。旁边的沙粒都嘲笑它，说它太傻，去蚌壳里住，远离亲人朋友，见不到阳光、雨露、明月、清风，甚至还缺少空气，只能与黑暗、潮湿、寒冷、孤独为伍，多么不值得！可那颗沙子还是无怨无悔地随养蚌人去了。斗转星移，几年过去了，那颗沙子已长成了一颗晶莹剔透、价值连城的珍珠，而曾经嘲笑它的那些伙伴们，有的依然是海滩上平凡的沙粒，有的已化为尘埃。
      
【温馨提示】

      如果说这世上有“点石成金术”的话，那就是“艰辛”。你忍耐着、坚持着、走完黑暗与苦难的隧道之后，就会惊讶地发现，平凡如沙子的你，不知不觉已长成了一颗珍珠。不要嫉妒珍珠，当初它选择珍珠的时候，别人都不愿意；也不必过分仰慕珍珠，毕竟每个人都有自己的人生，沙子也有沙子的幸福，虽然它不能闪光。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "企鹅的沉潜原则",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      南极的企鹅是种憨态可掬的小动物，可以在水中游嬉戏，也能在陆上行走。然而，南极大地的水陆交接处，全是滑溜溜的冰层或者尖锐的冰棱，它们身躯笨重，没有可以用来攀爬的前臂，也没有可以飞翔的翅膀，如何从水中上岸？在将要上岸之时，企鹅猛地低头，从海面扎入海中，拼力沉潜。潜得越深，海水所产生的压力和浮力也越大，企鹅一直潜到适当的深度，再摆动双足，迅猛向上，犹如离弦之箭蹿出水面，腾空而起，落于陆地之上，画出一道完美的U形线。这种沉潜是为了蓄势，积聚破水而出的力量，看似笨拙，却富有成效。
      
【温馨提示】

      甘于沉下去，才可浮出来，企鹅的沉潜原则，也适用于人的生存。人生又何尝不是如此？当我们面前困难重重，出头之日遥不可及时，何不学学企鹅的沉潜？这种沉潜绝非沉沦，而是自强。如果我们在困境中也能沉下气来，不被“冰棱”吓倒，在喧嚣中也能沉下心来，不被浮华迷惑，专心致志积聚力量，并抓住恰当的机会反弹向上，毫无疑问，我们就能成功登陆！反之，总是随波浮沉，或者怨天尤人，注定就会被命运的风浪玩弄于股掌之间，直至精疲力竭。",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "学会宽容",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一只小猪、一只绵羊和一头乳牛，被关在同一个畜栏里。有一次，牧人捉住小猪，他大声号叫，猛烈地抗拒。绵羊和乳牛讨厌它的号叫，便说：“他常常捉我们，我们并不大呼小叫。”小猪听了回答道：“捉你们和捉我完全是两回事，他捉你们，只是要你们的毛和乳汁，但是捉住我，却是要我的命呢!”
      
【温馨提示】

      立场不同、所处环境不同的人，很难了解对方的感受；因此对别人的失意、挫折、伤痛，不宜幸灾乐祸，而应要有关怀、了解的心情，要有宽容的心！",
            group8));

            group8.Items.Add(new SampleDataItem("Group-4-Item-3",
            "两个挑水喝的和尚",
            "Item Subtitle: 3",
            "Assets/title/8.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有两个和尚住在相邻的二座山上的庙里，这二座山之间有一条溪。于是这两个和尚，每天都会在同一时间下山去溪边挑水。久而久之，他们便成为好朋友了。就这样，时间在每天挑水中，不知不觉已经过了五年。突然有一天，左边这座山的和尚没有下山挑水。右边那座山的和尚心想:他大概睡过头了，便不以为意。哪知第二天，左边这座山的和尚，还是没有下山挑水。第三天也一样。过了一个星期，还是一样。直到过了一个月，右边那座山的和尚，终于受不了了。他心想:我的朋友可能生病了,我要过去拜访他，看看能帮上什么忙。于是他便爬上了左边这座山，去探望他的老朋友。等他到达左边这座山的庙看到他的老友之后，大吃一惊，因为他的老友，正在庙前打太极拳，一点也不像一个月没喝水的人。他好奇地问:“你已经一个月，没有下山挑水了，难道你可以不用喝水吗?”左边这座山的和尚说:“来来来，我带你去看。”于是，他带着右边那座山的和尚走到庙的后院，指着一口井说:“这五年来，我每天做完功课后，都会抽空挖这口井。即使有时很忙，能挖多少就算多少。如今，终于让我挖出井水，我就不必再下山挑水，我可以有更多时间，练我喜欢的太极拳。”
      
【温馨提示】

      学生课堂上所学只能算是平平常常的挑水喝，而丰富课外知识才是挖一口属于自己的井，培养自己多方面的实力。昨天的努力就是今天的收获，今天的努力就是未来的希望，别忘了现在就行动！",
            group8));




            this.AllGroups.Add(group8);
            #endregion

            #region 第9章
            var group9 = new SampleDataGroup("Group-9",
            "第9章",
            "Group Subtitle: 4",
            "Assets/title/9.png",
            "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
"天道酬勤——曾国藩与贼",
"Item Subtitle: 3",
"Assets/title/9.png",
"Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
@"
      曾国藩是中国历史上最有影响的人物之一，然他小时候的天赋却不高。有一天在家读书，对一篇文章重复不知道多少遍了，因为他还没有背下来。这时候他家来了一个贼，潜伏在他的屋檐下，希望等读书人睡觉之后捞点好处。可是等啊等，就是不见他睡觉，还是翻来覆去地读那篇文章。贼人大怒，跳出来说：“这种水平读什么书？”然后将那文章背诵一遍，扬长而去！
      曾先生后来启用了一大批人才，按说这位贼人与曾先生有一面之交，大可去施展一二，可惜他的天赋没有加上勤奋，名不经传变得不知所终，他只能成为贼，而曾先生却成为毛泽东主席都钦佩的人——近代最有大本夫源的人。
      
【温馨提示】

      勤能补拙是良训，伟大的成功和辛勤的劳动是成正比的，有一分劳动就有一分收获，日积月累，奇迹就可以创造出来。没有人能只依靠天分成功，上天给予了天分，勤奋将天分变为天才。",
group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "再试一次",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有个年轻人去微软公司应聘，而公司并没有刊登过招聘广告，见总经理疑惑不解，年轻人用不太娴熟的英语解释说自己是碰巧路过这里，就贸然进来了。总经理感觉这个年轻人有点儿胆识，破例让他一试。面试的结果出人意料，年轻人表现糟糕。他对总经理的解释是事先没有准备，总经理以为他不过是找个托词下台阶，就随口应道：“等你准备好了再来试吧。”。一周后，年轻人再次走进微软公司的大门，这次他依然没有成功。但比起第一次，他的表现要好得多。而总经理给他的回答仍然同上次一样：“等你准备好了再来试。”就这样，这个青年先后5次踏进微软公司的大门，最终被公司录用，成为公司的重点培养对象。
      
【温馨提示】

      什么东西比石头还硬，或比水还软？然而水滴石穿，坚持不懈而已。也许我们的人生旅途上沼泽遍布，荆棘丛生；也许我们追求的风景总是山重水复，不见柳暗花明；也许我们前行的步履总是沉重蹒跚；也许我们需要在黑暗中摸索很长时间才能找寻到光明；也许我们虔诚的信念会被世俗的尘雾缠绕而不能自由翱翔；也许我们高贵的灵魂暂时在现实中找不到寄放的净土……那么，我们为什么不可以以勇敢者的气魄，坚定而自信地对自己说一声“再试一次！”再试一次——你就有可能达到成功的彼岸！",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "锦囊?诗袋?瓦罐?百宝囊",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      李贺是中唐著名的诗人，又是从中唐诗风转变的一个代表者。李贺热爱诗歌，视诗歌为生命的一部分，为了把诗写好，他每天起得很早，背上饭和锦囊，骑上一匹瘦马，沿着一条小溪漫游。一路上，他细心观察和了解自然景物，即景吟诗，每逢想出佳句就写在纸条上，放入锦囊之中。母亲看着儿子那装满记有诗句纸条的锦囊，十分心疼地说：“唉呀！孩子啊，你把心呕出来才罢休吗？”可是李贺并不觉得辛苦，反而从中到很多乐趣，后来在李贺的不辞辛劳、精雕细琢下，他的诗篇千年传颂。
      宋代著名诗人梅尧臣，凡外出游玩或访亲友会时，总是随身带着一个号称“诗袋”的布袋，看什么新鲜的事或美丽的风景，有的得句，有的成诗，立即用笔在纸上记下，投入袋中。长此以往，梅尧臣的诗作获得了很高的成就。
      与李贺、梅尧臣相映成辉的是元末明初的文学家陶宗仪，他积累资料的办法既不是“锦囊”和“诗袋”，也不是现代的“卡片箱”，而是一只奇妙的“瓦罐”。他曾在松江隐居过，那时，他一边参加农业劳动，一边利用休息时间在树阴下捋摘树叶，记录他的所见所闻、所思所想，然后将这些“树叶”放到家中的瓦罐里。日积月累，年复一年，10年间积累了几十大罐。后来，他用这些资料写成了一部共有30卷的巨著《南村辍耕录》。
      托尔斯泰是俄国的大文章豪，他常常幽默地对别人讲，他有一个贮藏万物的“百宝囊”。他的“百宝囊”是什么呢？就是笔记本。他从来不离开他的“百宝囊”，日积累月，厚积薄发，从中变幻出一个又一个动人的形象。为了描写1813年战争时期俄国社会风气，他翻阅摘抄了70多种、包含几百卷的历史著作，连他自己家人也就成了一个藏书万卷的图书馆。
      被人们誉为“短篇小说小王”的俄国作家契诃夫，也有一个“百宝囊”，他还向人介绍说他的“百宝囊”里有“整整100个题材”。
      我国在作家老舍先生的“百宝囊”里储存着人物的语言、性格、外貌?????写作时需要什么就到这个“囊”里面找，可方便！
      
【温馨提示】

      做笔记：1.可以帮助记忆,俗话说“好记性不好烂笔头”；2.可以积累材料——学问越积越多；3.可以出成果——博采百花酿纯蜜。这些大文学家的好习惯为他们铺就了日后的成才之路，愿我们也能做个有心人，不断丰富自己的“锦囊”、“诗袋”、“瓦罐”、“百宝囊”----材料“仓库”，如此坚持下去，一定不会再为“没啥可写”而发愁。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "没有心的鹿",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      狮子和狐狸是一对要好的朋友，一天，勇猛的狮子不小心摔伤了，躺在山洞里，非常难受。他曾经听说吃了鹿心可以疗伤。于是对他的好友狐狸说：“我现在伤得很严重，需要鹿的心来治疗，就劳烦你把森林中最大的鹿骗到这里来。”
      狐狸无法推却狮子的要求，于是行动了，他走到树林里，看见树林里玩耍的大鹿，便向他问好，并说道：“我告诉你一个好消息。你知道，国王狮子是我的邻居，他伤得很厉害，看样子快要死了。他正在考虑，森林中谁能继承他的王位。他说野猪愚蠢无知，熊懒惰无能，豺子暴躁凶恶，老虎骄傲自大，只有大鹿才最适合当国王，鹿的身材魁梧，年轻力壮，他的角使蛇惧怕。我何必这么哆嗦呢？你一定会成为国王。这消息是我第一个告诉你的。如果你信任我的话，我劝你快去见他一面吧，他的重要的事情要和你交代。”经狐狸这么一说，大鹿被搞糊涂了，也没多想，便跟着狐狸来到山洞。一到山洞里，狮子猛然朝大鹿扑过来，用爪子撕下了他的耳朵。大鹿拼命地反抗，最终有幸逃回了树林??????
      狐狸辛辛苦苦白忙一场，他两手一拍，表示毫无办法了。狮子忍着疼痛，十分懊丧。他请求狐狸再想想办法，用诡计把大鹿再骗来。狐狸说：“你吩咐我的事太难办了，这是最后一次了。”
      于是，他花尽心思寻找大鹿的脚迹，心里不断盘数着坏主意。狐狸问其他动物是否见到一只带血的大鹿，他们告诉他大鹿在树林里。狐狸来到树林里，看到大鹿此时正在休息，就毫不羞耻地来到大鹿的面前。大鹿一见狐狸，气得毛都竖了起来，说：“坏东西，你休想再来骗我了！你再先靠近，我就对你不客气。”狐狸说：“你怎么这样胆小怕事？你难道怀疑我，怀疑你的朋友吗？狮子抓住你的耳朵，只是垂死的他想要告诉你一点儿关于王位的忠告与指示罢了。你却连那软弱无力的手抓一抓都受不住。现在狮子对你非常生气，要将王位传给狼。那可是一个坏国王呀！快走吧，不要害怕。我向你起誓，狮子决不会害你，我将来也专伺候你。”狐狸再一次欺骗了可怜的大鹿，并说服了他。
      大鹿跟着狐狸来到洞里，刚一进洞，就被狮子抓住饱餐了一顿，并把所有的骨头、脑髓和肚肠都吃光了。狐狸站一旁看着，鹿的心脏掉下来时，他偷偷地拿过来，把它当作自己辛苦的酬劳吃了。狮子吃完后，仍在寻找鹿的那颗心。狐狸远远地站着说：“鹿真是没有心，你不要找了。他两次走到狮子家里，送给狮子吃，怎么还会有心呢！”
      
【温馨提示】

      “人心隔肚皮。”也许初次和人打交道的时候上当吃亏很难避免，但是，在被骗一次之后，如果仍然不能从上次的经历中总结并吸取教训，再次上当受骗，就只能怪自己太愚蠢了。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "一偈得道",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      般特是释迦牟尼的弟子，他为人忠厚、善良，学习也够苦，释迦尼很喜欢他。但是般特生性有些愚钝。于是佛祖让五百罗汉轮流教他希望有一天他能成大器，可是他仍然没有什么大的长进。
      诲人不倦的佛祖把他叫到面前，不厌其烦、逐字逐句地教他一首偈：“守口摄意身莫犯，如是行者得度世。”
      佛祖说：“滴水落石出可以成海。你不要瞧不起这首偈，不要认为它过于简单，过于浅显，稀疏平常。如果你能真正学会这首偈，并能将其融会贯通，运用自如，那就是一件很不容易的事情了！”
      般听从了佛祖的教导，无论是严寒还是酷暑，般特都集中精力、精益求精地专学这首偈。功夫不负有心人，他日复一日地推敲琢磨，终于彻底领悟到了其中的含义与奥秘。
      般特的进步，佛祖看在眼里，喜在心上。过了一段时间，佛祖派他去给附近的女尼讲经说法。那些女尼早就听说过这个愚笨的般特，都瞧不起他，认为他不能胜任能讲经说法这个重要的工作。虽然她们心里是这样想，但表面上还是以应有的礼遇对待了。
      般特坦白而谦虚地对女尼说：“我生来愚钝，在佛祖身边只学得一偈，现在给大家讲讲自己的体会，希望静听。”
      接着便开始念偈：“守口摄意身莫犯，如是行者得度世。”
      话音刚落，女尼们哄堂大笑：“这实在是太简单了，真不值一提，我们都倒背如流了。”
      般特不动声色，从容不迫、认真地讲下去，讲得头头是道了，娓娓动听。一首看似普普通通的启蒙偈，竟让他深入浅出地讲出了无限深邃的佛理。
      女尼们听得如痴如醉，对般特顿时刮目相看，不禁心悦诚服地感叹道：“一首启蒙偈，居然可以理解、发挥到如此淋漓尽致的地步，实在是高人一筹啊！”
      佛祖给众弟子写了一幅字：“学不在多，贵在力行。”
      佛祖也给般特写了一幅字：“一偈得道。”
      般特虽然只懂一首启蒙偈，但这一首偈里却蕴藏着深邃的佛理。
      
【温馨提示】

      知识的获得很多时候不在于多少，而在于我们能否从容不迫有限的知识发现、总结出无限的哲理出来。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "要聪明但不要追求精明",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      聪明与精明的概念是不一样的，一字之别，差之千里。聪明也有限度，不要聪明反被聪明误。
      世界著名汽车制造商杜兰特手下的总裁叫卡洛?道尼斯。他曾是杜兰特手下的小职员，当他谈起他之所以被提升为总裁时说:“当我刚到杜兰特先生那儿工作时，我就注意到，每天下班后，所有的人都回家了，但杜兰特先生仍留在办公室里，一直呆得很晚。我认为应当有人留下来，给杜兰特先生提供一些工作上必要的协助，所以我也留下来了……他随时都能发现我……后来他就养成了召唤我的习惯。这就是整个事情的过程。”道尼斯所得到的报酬与他付出的时间精力肯定不相当，但是，这种吃亏却使他获得了总裁的位置。
      美国船王罗伯特?达拉有一位得力助手是位女士，最早她只是一名速记员。谈到她之所以能得到这个公司里所有女士都眼红的秘书位置时，罗伯特?达拉说“我在最初雇用她时，她的工作只是听取我的口述，记录内容，替我拆阅、分类及回复我的私人信件。她的薪水同公司其他普通的职员没什么两样。但是，同其他普通职员所不同的是，用完晚餐后，她还常常回到办公室来，并且积极地做那些本来不是她分内的、也没有报酬的工作．并把她替我写好的回信和其它一些文件送到我的办公室来。她的能力增长很快，有时候替我写的信就同我写的一样。当我的秘书因故辞职时，我自然而然地想到了她，因为她早已做着这样的工作，并且早已有了这样的能力。我多次提高她的薪水，直到她的薪水是普通职员的四倍。但是，这是没办法的事，她已经使她自己变得对我极有价值，是我的事业不能离开的帮手。”
      
【温馨提示】

      当你选择了一项事业并准备为之奋斗时，你一定要记住：要聪明，而不要追求精明。聪明的人一般不计较眼下的区区得失，而是把眼光放长远，时刻有一个总体的事业目标，所有的努力都是为这个目标而服务的。虽然他们的好多行为让别人看起来都是没有意义的，甚至很吃亏。但是他们心里清楚，自己的努力肯定在将来会得到巨大的利益回报。精明的人则总是很过敏地盯住眼前的利益，他们处人处事的标准是：人不利己，坚决不交；事不利己，坚决不为。这种人做事的风格给人的印象，好像他们并不是为了赚钱，而是怕别人拿走了他们的钱。这种人只有在不让自己吃亏的问题上，才能表现出才能和魄力。成功的人都是很聪明的人，最明白吃小亏占大便宜的道理；而精明的人最不服人，其事业用不了多长时间，就会失败。成功的人总是不惜血本来招揽人才，然后通过人才使他们成功；而失败的人总是因为不想吃亏，只想占便宜而失去人心，然后由人才匮乏和事业无助走向失败。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "齐白石学艺",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      齐白石是我国现代书画家和篆刻家。但他原是一位雕花木工，只在余暇学画和篆刻。
      年轻时候的齐白石就特别喜爱篆刻，但他总是对自己的篆刻技术不满意。一次，他向一位老篆刻师虚心求教，老篆刻师对他说：“你去挑一担础石回家，要刻了磨，磨了刻，等到这一担石头都变成了泥浆，那里你的印就刻好了。”于是，齐白石就按照老篆刻师的意思做了。他挑了一担础石来，一边刻，一边磨，一边拿古代篆刻艺术品来对照琢磨，就这样一直夜以继日地刻着。刻了磨平，磨平了再刻，手上不知起了多少个血泡。日复一日，年复一年，础石越来越少，而地上淤积的泥浆却越来越厚。最后，一担础石终于统统都“化石为泥”了。这坚硬的础石不仅磨砺了齐白石的意志，而且使他的篆刻技术也在磨练中不断长进，他刻的印雄健、洗练、独树一帜。渐渐地，他的篆刻技术达到了炉火纯青的境界。
      二十七岁那年，他的人生出现了重大转折。1889年春节的一天，书画家胡沁园给齐白石出了个画题，让他画一张横披。作完后，胡沁园十分惊喜，遂即收齐白石为徒。他教齐白石读唐宋诗，并引导他看小说。齐白石非常珍惜这个机会，常常读到深夜。经过几个月的苦读，齐白石背熟了《唐诗三百首》，还研读了不少古人涛文，浏览了许多古典名著。他作的诗也别具一格，具有唐风宋骨的韵味。胡沁园从“立意”、“用笔”等基本功入手教授齐白石，还把自己珍藏的古今名画借给他观摩。齐白石眼界大开，他揣摩“八大山人”的作品，临摩、领会其用笔之妙，吸取百家之长，绘画技艺突飞猛进，不足一年就掌握了山、水、人、物、花、鸟的基本画法和技巧。在老师的言传身教下，他苦练书法和刻印。短短几年时间，齐白石在绘画、篆刻、吟诗、书法、装裱等方面都取得了惊人的进展，成为名满天下的书画家。
      
【温馨提示】

      既然石头都可以磨成泥浆，学习上的困难有什么克服不了的呢？毅力可以转化神奇的力量，帮助我们达到成功的彼岸，从而让动力的人生绽放芬芳的花朵！而毅力则来自于一个人乐观的心态，不管顺境、逆境，都乐观地对待，我们总会有意外的收获。“我无畏狂风暴雨，因为我学会如何航行。”",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "没有鱼鳔的鱼",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      从前，有一个年轻人，因为家贫没有读多少书，他去了城里，想找一份工作。可是他发现城里没一个人看得起他，因为他没有文凭。就在他决定要离开那座城市时，忽然想给当时很有名的银行家罗斯写一封信。他在信里抱怨了命运对他是如何的不公。他说：“如果您能借一点钱给我，我会先去上学，然后再找一份好工作。”
      信寄出去了，他便一直在旅馆里等，几天过去了，他用尽了身上的最后一分钱，也将行李打好了包。就在这时，房东说有他一封信，是银行家罗斯写来的。可是，罗斯并没有对他的遭遇表示同情，而是在信里给他讲了一个故事：
      在浩瀚的海洋里生活着很多鱼，那些鱼都有鱼鳔，但是唯独鲨鱼没有鱼鳔。没有鱼鳔的鲨鱼照理来说是不可能活下去的。因为它行动极为不便，很容易沉入水底，在海洋里只要一停下来就有可能丧生。为了生存，鲨鱼拥有了强健的体魄，成了同类中最凶猛的鱼。
      最后，罗斯说：“这个城市就是一个浩瀚的海洋，拥有文凭的人很多，但成功的人很少。你现在就是一条没有鱼鳔的鱼……”
      那晚，他躺在床上久久不能入睡，一直在想着罗斯的信。突然，他改变了决定。第二天，他跟旅馆的老板说，只要给一碗饭吃，他可以留下来当服务生，一分钱工资都不要。旅馆老板不相信世上有这么便宜的劳动力，很高兴地留了他。10年后，他拥有了令全美国羡慕的财富，并且娶了银行家罗斯的女儿，他就是石油大王哈特。
      
【温馨提示】

      有时阻止我们前进的不是贫穷，而是优越。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "父亲和两个儿子的故事",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      故事一：
      父亲欲对一对孪生兄弟作“性格改造”，因为其中一个过分乐观，而另一个则过分悲观。一天，他买了许多色泽鲜艳的新玩具给悲观孩子，又把乐观孩子送进了一间堆满马粪的车房里。
      第二天清晨，父亲看到悲观孩子正泣不成声，便问：“为什么不玩那些玩具呢?”
      “玩了就会坏的。”孩子仍在哭泣、
      父亲叹了口气，走进车房，却发现那乐观孩子正兴高采烈地在马粪里掏着什么。
      “告诉你，爸爸。”那孩子得意洋洋地向父亲宣称，“我想马粪堆里一定还藏着一匹小马呢！”
      
【温馨提示】

      乐观者与悲观者之间差别是很有趣的：乐观者看到的是油炸圈饼，悲观者看到的是一个窟窿；乐观者在每次危难中都看到了机会，而悲观的人在每个机会中都看到了危难。故事二：父亲嗜酒如命，还染上了毒瘾。后来，因为在酒吧里杀害了一位酒保而被判死刑，只留下两个儿子孤零零地活在世上。父亲死后，两兄弟相依为命。但是，其中的一个跟父亲一样染上毒瘾，还因偷窃和勒索、伤人坐了牢。另外一个儿子可不一样，他既不喝酒也不吸毒，靠打工挣钱完成学业，还成了一家公司的分区经理。他的婚姻也很美满，妻子为他生了三个可爱的孩子。出自同一个父亲，在完全相同的环境下长大，但两人的命运却截然不同，这是为什么呢？面对这个问题，两兄弟的回答竟是完全相同：“有这样的父亲我还能有什么办法？”",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "责任与爱",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      五岁的汉克和爸爸妈妈哥哥一起到森林干活，突然间下起雨来，可是他们只带了一块雨披。爸爸将雨披给了妈妈，妈妈给了哥哥，哥哥又给了汉克。汉克问道：“为什么爸爸给了妈妈，妈妈给了哥哥，哥哥又给了我呢？”爸爸回答道：“因为爸爸比妈妈强大，妈妈比哥哥强大，哥哥又比你强大呀。我们都会保护比较弱小的人。”汉克左右看了看，跑过去将雨披撑开来挡在了一朵风雨中飘摇的娇弱小花上面。
      
【温馨提示】

      真正的强者不一定是多有力，或者多有钱，而是他对别人多有帮助。责任可以让我们将事做完整，爱可以让我们将事情做好。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "期望值",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      两匹马各拉一辆大车。前面的一匹马走得很好，后面的那匹马常常停下来。主人以为这匹马拉不动了，于是，就试着把后面那辆车上的货物挪到前面那辆车上去。前面的这匹马说：“主人，你就把那东西全都放到我车上吧，我拉得动。”等到后面那辆车上的东西都搬完了，后面那匹马便轻快地前进，并且对前面那匹马说：“你使劲干吧，流汗吧，你越是努力干，人家越是要折磨你。”来到车马店时，主人说：“这匹马真能干。既然只用一匹马拉车就够了，我养两匹马干嘛，不如只喂这一匹，把另一匹杀掉，总还能拿到一张马皮。”于是，主人就真的这样做了。后面那匹马被杀死了。
      
【温馨提示】

      自己对自己的期望值，时刻都要比别人对自己的期望值高。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "小心“牛屎运”",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一只火鸡和一头牛闲聊。
      火鸡说：“我希望能飞到树顶，可我没能力气。”
      牛说：“为什么不吃一点牛粪？它是很有营养的。”
      火鸡吃了一些牛粪，它有了力量飞上了第一根树枝。
      第二天，火鸡吃了更多的牛粪，飞到第二根树枝。两个星期后，火鸡骄傲地飞到了树顶。但是不久，一个猎人看见了高高站在树顶的火鸡，一枪把它射了下来。
      
【温馨提示】

      “牛屎运”可以让你达到顶峰，但不可能让你永远留在那里。一只小鸟飞到南方去过冬。天太冷，小鸟快被冻僵了，于是它飞到一大块空地上。一头牛经过那儿，拉了一堆牛粪在小鸟的身上。快冻僵的小鸟躺在粪堆里，觉得好温暖，渐渐苏醒过来。它温暖而快活地躺着，不久开始唱起歌来。一只路过的猫听到歌声，便走过去看个究竟。循着歌声，猫很快发现了粪堆里的小鸟，于是就把它拽出来，然后把它吃掉了。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "小丑与公主",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      国王心爱的女儿病了，她说要能拿到月亮病就会好。国王立刻召集天下聪明智士，要他们想办法拿到月亮，但无论是总理大臣、宫廷魔法师，还是宫廷数学家，谁都没有办法。而且，他们分别对拿月亮的困难有不同的说辞：总理大臣说它远在三万五千里之外，比公主的房间还大，而且是由熔化的铜组成的；魔法师说它有15万里远，用绿奶酪做的，而且大小整整是皇宫的两倍；数学家说月亮远在30万里之外，又圆又平，像个钱币，有半个王国大，还被粘在天上，不可能有人能够把它拿下来。国王面对这些“不可能”，又烦又气，只好叫宫廷小丑给他弹琴解闷。
      小丑问明了一切后，得出了一个结论：如果这些有学问的人说得都对，那么月亮的大小一定和每个人想的一样大、一样远。所以，当务之急是弄清楚小公主心目中的月亮有多大、有多远。国王一听，茅塞顿开，吩咐小丑解决这个难题。小丑立即到公主的房里探望她，问公主：月亮有多大？”“大概比我拇指的指甲小一点儿吧！”公主说，因为她只要把拇指的指甲对着月亮就可以把它遮住了。“那么有多高呢？”“不会比窗外的那棵大树高！”公主之所以这么为，因为有时候它会卡在树梢间。“用什么做的呢？”“当然是金子！”公主斩钉截铁地回答。比拇指指甲还要小、比树还要矮，用金子做的月亮当然容易拿啦！小丑立时找金匠打了一个小月亮、穿上金链子，给公主当项链，公主高兴极了，没几天病就好了。但是国王仍旧很担心。到了晚上，真月亮还是会挂在天上，如果公主看到了，谎言不就被揭穿了吗？于是，他又召集了那班“聪明人”，向他们征询解决问题的方法，怎样才能不让公主看见真正的月亮呢？有人说让公主戴上墨镜，有人说把皇宫的花园用黑绒布罩起来，有人说天黑之后就不住地放烟火，以遮蔽月亮的光华……当然，没一个主意可行。怎么办？心急的国王深恐小公主一看见真月亮就会再次生病，但又想不出解决方法，只好再次找来小丑为他弹琴。小丑知道了那些聪明大臣的想法后，告诉国王，那些人无所不知，如果他们不知道怎样把月亮藏起，就表示月亮一定藏不住。这种说辞，只能让国王更沮丧。眼看着月亮已经升起来了，他看着就快照进公主房间的月亮，大叫：“谁能解释，为什么月亮可以同时出现在空中，又戴在公主的脖子上？这个难题谁能解？”小丑于是他就赶到了公主的房间，向公主提出了这个问题。没想到公主听了哈哈大笑，说他笨，因为这个问题太简单了，就像她的牙齿掉了会长出新牙，花园的花被剪下来仍会再开一样，月亮当然也会再长出来啦。小丑因此而受到国王的嘉奖，成为宫中大臣。
      真正的小丑其实是总理大臣、宫廷魔法师、宫廷数学家等这些所谓的智者。
      
【温馨提示】

      亚里斯多德说过：自然界选择最简单的道路。把简单的事情复杂化很容易，把复杂的事情简单化却很难。简单往往是解决问题的最佳途径。“知己知彼”是“百战百胜”的基础。智谋、学识和权力，在孩子面前都会失去力量，唯一能抵达孩子内心的途径就是那颗孩子一样纯净天真的心灵。在儿童的世界里道貌岸然的成人才是真正的小丑。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "永远保持一双明亮的眼睛",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一场雷电引发的山火烧毁了美丽的“森林庄园”，刚刚从祖父那里继承了这座庄园的保罗陷入了一筹莫展的境地。百年基业，毁于一旦，怎不叫人伤心。保罗决定倾其所有修复庄园，于是他向银行提交了贷款申请，但银行却无情地拒绝了他。再也无计可施了，这位年轻的小伙子经受不住打击，闭门不出，眼睛熬出了血丝，他知道自己再也看不见曾经郁郁葱葱的森林了。
      一个多月过去了，年已古稀的外祖母获悉此事，意味深长地对保罗说：“小伙子，庄园成了废墟并不可怕，可怕的是，你的眼睛失去了光泽，一天一天地老去，一双老去的眼睛，怎么能看得见希望……”保罗在外祖母的说服下，一个人走出了庄园。
      深秋的街道上，落叶凋零一地，一如他零乱的心绪。他漫无目的地闲逛，在一条街道的拐弯处，他看到一家店铺的门前人头攒动，他下意识地走了过去。原来是一些家庭主妇正在排队购买木炭。那一块块木炭忽然让保罗的眼睛一亮，他看到了一丝希望。在接下来的两个星期里，保罗雇了几名炭工，将庄园里烧焦的树木加工成优质的木炭，分装成1000箱，送到集市上的木炭经销店。结果，木炭被抢购一空，他因此得到了一笔不菲的收入，然后他用这笔收入购买了一大批新树苗。几年以后，“森林庄园”再度绿意盎然。
      
【温馨提示】

      一把火可以烧毁的只是我们一时的希望，即使在一片死灰里同样可能蕴藏着生机，无论面对什么，只要我们都能永远保持一双明亮的眼睛，对你来说处处都有转机。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "从你最近的目标开始做起",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位青年满怀烦恼的去找一位智者。他大学毕业后，曾豪情万丈地为自己树立了许多目标，可是几年下来，依然一事无成。他找到智者时，智者正在河边小屋里读书，智者微笑着听完青年的倾诉，对他说：'来，你先帮我烧壶开水！'
      青年看见墙角放着一把极大的水壶，旁边是一个小火灶，可是没发现柴火，于是便出去找。他在外面拾了一些枯枝回来，装满一壶水，放在灶台上，在灶内放了些柴火便烧了起来。可是由于壶太大，那捆柴火烧尽了，水也没开。于是他跑出去继续找柴火，等找到了足够的柴火回来，那壶水已凉得差不多了。这回他学聪明了，没有急于点火，而是再次出去找了些柴火。由于柴火准备得足，水不一会儿就烧开了。
      智者忽然问他：'如果没有足够的柴火，你该怎样把水烧开？'
      青年想了一会儿，摇摇头。
      智者说：'如果那样，就把壶里的水倒掉一些！'
      青年若有所思地点了点头。
      智者接着说：'你一开始踌躇满志，树立了太多的目标，就像这个大壶装的水太多一样，而你又没有足够多的柴火，所以不能把水烧开。要想把水烧开，你或者倒出一些水，或者先去准备柴火！'
      青年顿时大悟。回去后，他把计划中所列的目标划掉了许多，只留下最近的几个，同时利用业余时间学习各种专业知识，几年后，他的目标都实现了。
      
【温馨提示】

      只有删繁就简，从最近的目标开始，才会一步步走向成功。万事挂怀，只会半途而废。另外，我们只有不断地捡拾那些'柴火'，才能使人生逐渐加温，最终才会让生命沸腾！",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "改变人生命运的两个字",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      记得那是一个寒冬的下午，我坐在曼哈顿的一个法国小餐馆等人，我当时的心情十分郁闷。由于几次失算，我把有生以来一个相当重要的项目给搞砸了。我的老朋友说道：“现在的问题是，光说‘要是’不能解决任何问题。光说‘要是’只能让人把握错自己的方向——向后看而不是向前看。光说‘要是’只是浪费光阴。要是一味这么下去的话，最后，说‘要是’就会成为自己的积习，这么一来，‘要是’就会成了真正的绊脚石，成了自己不再尝试的托词。”
      “现在我们以你为例：你的方案没有成功，为什么？因为你犯了一些错误。不过，这并没什么关系，犯错误人皆难免，我们都得从错误中吸取教训。但是，刚才你在向我讲述自己的错误，一时悔恨这个，一时抱怨那个，那就说明你并没有真正从错误中吸取教训。”
      “那你凭什么这么说我呢？”我为自己辩白，但有些底气不足。我的老朋友接着说道：“因为，这表明你从未从过去的阴影中走出来。你对未来只字未提。而且，老实讲，从某种程度上来说，你当时还陶醉于谈论自己的过去。我们大家都有一个共同的人性弱点，那就是，我们都乐于回味自己以往犯过的一些错误。毕竟，自己讲述遭受过的灾难和挫折时，自己依然是主角和舞台的焦点。”
      我悔恨不已，摇了摇头，问道：“那么，有什么补救的法子吗？”
      “有啊，转移焦点，”我的老朋友立即答道，“换掉关键字，换上一个催人奋进而不是令人不思进取的词。”
      “那你有什么高见吗？”
      “当然有！去掉‘要是’，换上‘下次’。”
      “下次？”
      “一点不错！在这房间里，我已多次看到它创造出小小的奇迹。只要病人反复对我说‘要是’，那他就麻烦了。不过，假如他能正视着我，说‘下次’，我便知道他即将渡过难关了，因为这意味着：不论以往多么艰辛或多么痛苦，他已决定运用从往事中吸取的教训了；这意味着：他即将推开悔恨这一绊脚石，振作起来，采取行动，重新生活。你自己也尝试一下看看，你会明白。”我的老朋友不再说了。此时，我能听到窗外雨点溅落在窗户玻璃上的声音。我试着从大脑中慢慢驱出一个词并替换上另一个词。当然，这只是在想象之中进行的，但我却清晰地听到新词在大脑中相应之处安家落户。
      “还有，对于能够补救的事情，那就不妨试一试此法。”然后，我的老朋友从身后的书架上抽出一本日记一样的东西，接着说道：“这是一本日记，日记的主人是十几年前我家乡的一位老师。她的丈夫为人随和，但游手好闲，虽然长得好看，但是养家糊口却完全不称职。这么一来，养儿育女和其他一切责任全落在她的肩上。她的日记充满气愤，尽是数落乔纳森的弱点、缺陷和不足。后来，乔纳森死了，此后多年她也再也没有提及此事，只有一次例外。你看这里：今天我当选学监，所以我认为我理当自豪。然而，要是我知道乔纳森在天上哪个地方，而且要是能够到他身边的话，我今夜就会去他那里。”
      老朋友轻轻地合起了日记，说道：“明白了吧？她反复在说‘要是’。‘要是’我接受了他，包括他所有的缺点；‘要是’我在当初爱他的话，那该有多好啊！”他把书放回书架，接着说道：“这是这两个令人伤心的字说得最令人伤心的时候，因为此时一切为时已晚，回天无力。”我的老朋友有些僵硬地站了起来，说道：“好啦，下课了！小伙子，见到你，我非常开心，一直都是如此！现在，请你帮我拦辆出租车，或许我也该回家了。”
      我们走出大楼，进入雨夜。我看到一辆空车在招揽客人，便向前跑了一下准备拦下，可是却被另一个行人捷足先登了。“哎呀，哎呀，‘要是’我们早来一步的话，我们就会拦到那辆出租车了，你说对不对？”我的老朋友狡黠地说道。我会心地笑了笑，说道：“下次我再跑快点。”“对啦！这就对啦！”说着，我的老朋友拽了拽自己那滑稽的帽子，盖在自己耳朵的四周。又一辆出租车放慢了车速。我为我的老朋友打开了车门。车子慢慢地开走，他冲我笑了笑，挥了挥手。
      而今，那个曼哈顿雨夜已过去一年多了。然而，时至今日，每当我脑海想“要是”这个词的时候，我马上换上“下次”一词。然后，我等待着那几乎听得到的“喀嚓”声。而每当听到这个声音时，我就想起我那位老朋友。当然，这只是永恒中的一个小小插曲，他要是在世的话，他也希望我如此。
      
【温馨提示】

      现在开始把失败后的“要是”换成“下次”，抓住人生的下一次机会......",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "上帝的金币",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一天，上帝经过一座城市时，从高处看到一家豪华酒店的门外徘徊着三个乞丐，他们伸长脖子，对那些进进出出的富人流露出无比的神往。那神情让上帝动了侧隐之心，便对他们说：“我是上帝，我可以给你们每人一袋金币。”乞丐们顿时跪倒在地，纷纷对上帝磕头。上帝说：“不过，有个条件，你们只能用身上装乞讨物的布袋来装，装多少都行，但金币一旦掉到地上，金币就不是金币了，而成了石头。你们明白了吗?”乞丐们纷纷解下身上的布袋，敞开来，准备装上帝给的金币。
      上帝伸出手来，罩在第一个人的布袋上空。上帝说：“你说停我就停，明白吗?”第一个人说：“明白。”“开始!”上帝一声叫，他的手掌下，金币就像雨一样落进布袋。当金币落满半布袋时，上帝提醒第一个人说：“够了吧，这么多金币够你几辈子花了，而且金币是很沉重的，一旦布袋破了，就竹篮子打水一场空。”第一个人却连声道：“没事的，没事的。”结果，他话音未落，布袋承受不了金币的重量，袋底破了，所有的金币掉落在地上，化为石头。第一个人沮丧极了，他为自己的贪婪付出了昂贵的代价。“可惜了!”上帝说罢，转向第二个人，伸出手去。
      金币叮叮当当地落进第二个人的布袋里。上帝不断地提醒道：“够你一辈子花了，够你两辈子花了，够你三辈子花了……”第二个人连忙说停，金币已装满大半袋。上帝说：“可喜!可喜!”
      接着是第三个人，但上帝刚落下去一枚金币，他就叫停。上帝叹息道：“这太可惜了!难道你不想拥有更多的金币吗?”第三个人说：“我做梦都想，但任何东西来得容易去得也快。”
      多少年后，上帝突然想到这三个乞丐，就叫天使去查一下他们现在在哪儿。不久，天使把这三个人带到上帝的跟前。原来，这三个人都已经上了天堂。
      上帝就问三人，他们各自的生活过得怎么样。
      第一个人说：“我悔恨成疾，不到一年便抑郁而死。”
      第二个人说：“我回老家买了座城堡，娶了三房太太，正准备购置一大笔房地产时，结果因露富而遭人暗算，丢了性命。”
      轮到第三个人时，他将手中紧握的那枚金币还给上帝。他说，他一直将上帝的金币带在身边，自始至终都感到上帝与自己同在。为在芸芸众生之中，唯有两个人得到上帝的金币，而他是其中的一个。他创立了一番事业，并功成名就，儿孙满堂，活到99岁才无疾而终。
      上帝接过金币，微微一笑，然后把那枚金币递给第一个人和第二个人看，说：“你们好好看看，这才是我的金币。”
      
【温馨提示】

      知足是难得的品质。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "醉心花草之中的孩子",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      从前，有一个孩子，在学校的功课差极了，老师说他的智力有问题，孩子们不和他一起玩。看上去，他的确有些沉默寡言，经常一个人在屋前的花园里很长时间地看着花草小虫，自得其乐。他的父亲教训他：“除了打猎、养猫和狗、种花草以外，你什么都不操心，将来会有辱你自己，也会有辱你的整个家庭。”他的姐姐也看不起这个学习成绩平平、行为怪异的兄弟，他在家庭中是一个不受欢迎的人。
      但是他的母亲怜悯他，她想如果孩子没有那些乐趣，不知道他的生活还有什么色彩，她对丈夫说：“你这样对他不公平，让他慢慢学着改变吧。”丈夫说：“你这是怜悯，不是教育，你会毁了他一生。”但他、她却固执已见，他是她的孩子，需要她的安慰和鼓励。
      她支持孩子到花园中去，还让孩子的姐姐也一同去。母亲耍了一个小心机，她对孩子和他的姐姐说：“比一下吧，孩子，看谁从花瓣上先认出这是什么花？”儿子总是比他的姐姐认得快，母亲每次都吻他一下。这对孩子来说，是多么令人兴奋的一件事，他回答出了姐姐无法回答的问题。他开始整天研究花园里的植物、蝴蝶，甚至观察到了蝴蝶翅膀上斑点的数量。”
      对于孩子母亲的做法，她的丈夫始终觉得不可理喻，父亲认为那是对儿子的放任是怜悯是无助于成长的，除了暂时麻醉孩子之外，根本毫无益处。但是，就是这位醉心于花草之中的孩子，多年后成为了生物学家，创立了著名的“进化论”。他就是19世纪英国最伟大的博物学家、生物学家查理.罗伯特.达尔文。
      
【温馨提示】

      所有的父母都爱自己的孩子，但只有极少数父母爱得正确。爱自己的孩子，就要在乎孩子怎么想，而不是自己怎么想。爱是没有对错的，但给予的方法一定有对错之分。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "为一只蝴蝶失去一个大陆",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      澳大利亚是一个美丽的国家，它这块“新大陆”是在18世纪末被发现的。这一消息很快被探险家们带回欧洲。对于这块“新大陆”许多国家都想据为已有。1802年，英国和法国各派出一支船队驶向“新大陆”，想占领这块宝地。英国方面由弗林斯达船长带队，法国方面则由阿梅兰船长领军，两位船长都是长期叱咤海上、经验异常丰富的航海家。双方都知道对方也派出了占领船队，因此都不甘示弱，拼抢非常激烈。
      当时法国方面的船只技术较为先进。阿梅兰船长率领的三桅快船捷足先登，第一个到达了今天澳大利亚维多利亚港，并将它命名为“拿破仑领地”。正在他们准备插旗扎寨之时，突然发现了当地特有的一种珍奇蝴蝶，于是兴高采烈地法国人全体出动，一齐去抓这种蝴蝶。
      巧合的是，就在法国人深入大陆腹地猛追蝴蝶的同时，英国人也来到了这里。当法国船队的船映入他们眼帘时，英国船员们都以为法国人已经占领了此地，心情无比沮丧。弗林斯达舰长命令部属登岸，准备有风度地向法国人祝贺。谁知到了岸上一看，既看不到法国人的踪影，也看不到任何占领标志。于是，英国人立即紧急行动起来，把大英帝的名种标志插得遍地都是。
      当法国人带着漂亮的蝴蝶标本回来时，却吃惊地发现，他们的“拿破仑领地”已经不复存在了，英国人正严阵以待，俨然以胜利者的姿态向他们介绍“维多利亚”领地的归属。
      为一只蝴蝶失去了一个大陆。澳大利亚就这样在一天之内完成了由法属殖民地向英联帮体系的过渡。留给浪漫的法国人的，只能是一些可怜的蝴蝶标本和无尽的沮丧。
      
【温馨提示】

      很多同学都说这次期末考试没有达到目标，其实仔细想想你在实现目标的过程中是否抓住了重点，是否做得够好？做什么事情都要明确目标，从大局出发，分清轻重缓急，决不能为了一些微不足道的事情而放弃或转移目标。否则，就会像法国人一样为一只蝴蝶失去一个大陆，到头来自己得到的只能是沮丧和懊悔。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "胡萝卜、鸡蛋和咖啡豆",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一天，儿子和父亲抱怨对生活的不满：“我真不知道该如何生活了？我觉得生活和学习和压力太大了一个问题刚解决，另一个问题又出现了，简直超出我能承受的极限。”父亲低头想了想，然后叫儿子跟他进了厨房。儿子不解其意，只看见父亲将胡萝卜、鸡蛋和咖啡豆分别放进三口锅中，并在三口锅中放同样多的水，然后把它们放在火上烧。
      水开了十几分钟后，他把煮好的胡萝卜、鸡蛋和咖啡分别放到碗、碟和杯子里，然后指着它们问儿子：“告诉我你看见了什么？”儿子满脸不屑地回答：“真是个愚蠢的问题，那不是胡萝卜、鸡蛋和咖啡吗？”
      父亲点点头说：“那你去摸胡萝卜。”儿子告诉父亲胡萝卜变软了；
      “那你打碎鸡蛋看看。”儿子将鸡蛋壳剥掉后，看到的是只煮熟的鸡蛋；
      “孩子喝杯咖啡吧。”儿子端起咖啡一饮而尽，“好香香甜的咖啡呀！”儿子回答到。
      这时，父亲笑着对儿子说：“胡萝卜是强壮的，但经水煮后却变软了、变弱了,失去了原本的硬朗与蔬菜清香；鸡蛋原先是易碎的，经水煮后却变硬了；而咖啡豆在进入沸水后则改变了水,让水感染上了它的芳香。儿子，人的潜力是无穷的！”儿子恍然大悟。
      
【温馨提示】

      世上的人可以分为三种：一种像胡萝卜，一种像鸡蛋，而另一种像咖啡豆，都在人生这口大锅中的热水中浸泡、磨砺着。人的一生也可以比喻成胡萝卜、鸡蛋和咖啡豆三个不同的成长阶段：一开始遇到困难就逃避而变软弱（像胡萝卜）；后来逐渐学会自我保护变坚强（像鸡蛋）；最后学会解决困难（像咖啡豆）。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "勇敢地说“不！”",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      小泽征尔是“世界三大东方指挥家”之一。他指挥的曲目十分广泛，包罗万象，古往今来，无所不晓。
      一次，他去欧洲参加指挥家大赛，决赛时，他被安排在最后。评委交给他一张乐谱，小泽征尔稍做准备就全神贯注地指挥起来。
      突然，他发现乐曲中出现了一点不和谐，开始他以为主演奏错了，就指挥乐队停下来重奏，但仍觉得不自然，他感到乐谱确实有问题。可是，在场的作曲家和评委会权威人士都声明乐谱不会有问题，是他错觉。面对几百名国际音乐界权威，他不免对自己的判断产生了动摇。但是，他考虑再三，坚信自己的判断是正确的。于是，他大声说：“不！一定是乐谱错了！”他的声音刚落，评席上那些评委们立即站起来，向他报以热烈的掌声，祝贺他大赛夺魁。
      原来这是评委们精心设计的一个“圈套”，以试探指挥家在发现错误而权威人士不承认的情况下，是否能够坚持自己的判断，因为，只有具备这种素质的人，才真正称得上是世界一流的音乐指挥宾家。在三名选手中，只有小泽征尔相信自己而不附和权威的意见，从面获得了这次世界音乐指挥家大赛的桂冠。
      面对数百名权威故意设下的圈套，小泽征尔大胆地说了“不”，并最终赢得了比赛。
      
【温馨提示】

      在许多时候，质疑权威，是需要很大的勇气的，保持这种勇气，为我们自己赢得成功。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "找出关键点",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一次，美国福特汽车公司的一台大型了动机了发生了故障，为了排除故障，福特公司请了很多人来修，可都有束手无策，最后请来了德国著名的电机专家斯坦门茨。
      斯坦门茨围着机器转了两圈后，用粉笔在电机外壳的某处画一个“X”，然后吩咐公司负责人说：“把做记号处的线匝减少16匝。”负责人照斯坦门茨说的做了，难题迎刃而解。斯坦门茨索要了1万美元的报酬。
      许多人不解议论纷纷，说画一个“X”就要1万美元，实在是太多了。斯坦门茨回答道：“用粉笔画一个‘X’值1美元，知道在哪里画‘X’值9999美元。“
      此语一出，众人皆默然。
      
【温馨提示】

      画“X”是人人都能做到的，知道具体在哪里画“X”却是极少数人才具备的才能。许多人都会质疑，可是却很少有人会有心底问过自己是否具有质疑的学识和能力，以及质疑的要点是否抓住敢。我们在提出疑问时，要善于抓住关键点，才会有价值。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "失败者任其失败，成功者创造成功",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      在1964年，现代运动史上发生了一件很重要的事件。那时，诺马士预测他的球队美国足球联盟纽约喷射机队，在第三届超级杯足球赛中，会打败国家足球联盟的巴尔的摩小马队。当时如此的预测似乎全然不按章法而行，原因是：首先，小马队被专家预计会赢十九分，因为前两届超级杯，也都是国家足球联盟代表队，轻取美国足球联盟的球队。但是让人对诺马士的预测感到震惊的是，当时根本没有球员会在赛前谈论此事，当喷射机队获胜时，人们除了感觉惊讶之外，还为他觉得松了口气，因为全国的人都准备把诺马士痛揍一顿，他竟敢如此信口雌黄。如果喷射机队输了，他即使不搬到西伯利亚那么远的地方，也得暂避至南美洲。自此情形大变，诺马士创造了一个新的趋势。
      同样，拳王阿里也曾用过这种方法——向自己挑战极力自己发挥出更大的潜能。数年后，在拳王阿里与弗来奇尔对阵之前，他像诺马士那样宣称自己将获得胜利。同样的，这种装腔作势似乎不按牌理出牌在他早期的拳击生涯中，阿里常常预测对手的势力，但那时他是与势力远不如他人的人竞赛。现在，阿里是离开圈内多年湖在战，而弗来奇尔则是常胜将军。阿里居然仍夸口自己会胜利，他也说了不止一次，还重复无数次。这回，他的预测错了，阿里输了，最后一站他辛苦应战，但失败了。在这之后不久，阿里被邀请上美国一家电视台的访谈节目，在他被介绍给观众之前，有人怀疑他上台时观众会有何反应。他曾信誓旦旦地说他一定会赢，结果他输了，那的确令人无地自容。可是当阿里出现时，他受到在场观众真诚地气力致意，热烈鼓掌喝彩。他并不被认为是个愚弄的自己的人，相反的，认为是一名勇于以自己的名誉做赌注的勇士，虽然比赛结果并未如他所言，但比起他甘冒大险的勇气，胜负真如鸿毛一般，不值一提。
      当然，在诺马士和阿里的时代之后，那种运动员在赛前夸张的预测的，已全然没有价值，因为它确实毫无意义。但是，如果你有胆量说你掌握自己的命运，这个世界将因此而尊敬你，即使事后证明你错了。
      唯一蝉联三次世界冠军的天才教练蓝柏第有一次说：“任何一位顶天立地、有作为的人，不管怎样，最后他的内心一定会感谢刻苦的工作与训练，他一定会衷心向往训练的机会。”
      
【温馨提示】

      失败者任其失败，成功者创造成功。胜利者天生是倾向行动的人，他们将自己视为世界舞台的演员，而非被动地被他人行动牵制的受害者。即使失败了，胜利者也宁愿选择一种有声有色的方式失败。如果无法避免失败，就轰轰烈烈的大干一场。这是成功者的处事方法。毅力可以克服阻碍。当你继续迈向高峰时，必须记住：每一级阶梯都供你踩足够的时间，然后再踏上更高一层，它不是供你休息之用。我们在途中难免会疲倦与灰心，但就像世界重量级冠军詹姆士.柯比常说的：“你要再占一回合才能获胜，碰上困难时，你要再战一回合。”每一个人都有无限的潜能，但除非你知道他在哪里，并坚持用它，否则毫无价值。世界著名的大提琴演奏家帕柏罗卡沙成名后，仍每天练习6个小时，有人问他为什么还呀这么努力，他的回答是：“我认为我正进步之中。”每一个“不”的回答都使你愈来愈接近“是”的目标。“黎明前总是黑暗”，这句话并非口头禅，只要你努力学习，发挥你的特长与天才，成功的一天终会到来。需要指出的是，即使你成功的一天永远没有到来，你仍然是个大赢家，因你已经有了知识，也懂得面对人生，那是更大的成功。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "你的头脑是你最有用的资产",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      成功有捷径吗？成功学大师拿破仑?希尔的回答是肯定的。致富的捷径就是以积极的思考致富，相信你能，你就做得到，不论你是谁，不论年龄大小，不论学问程度高低，都能招揽财富。
      乔治?哈姆雷特在伊利斯诺州的一家退伍军人医院疗养，休闲的时间很多，可是能做的事情并不多，他就静下心来读书思考。乔治知道很多干洗店在烫好的衬衣上加上一张硬纸板，防止变形，他写了几封信向厂商洽询，得知这种硬纸板的价格是每千张4美元。他在静静的思考中突发灵感，在硬纸板上加印广告，再以每千张1美元的低价卖给洗衣店，赚取广告的利润。乔治出院后，立刻着手进行，并持续每天研究、思考、规划。广告推出后，乔治发现客户取回干净的衬衣后，衣服的纸板丢弃不用。乔治问自己：“如何让客户保留这些纸板和上面的广告？”答案闪过他的脑际。他在纸板的正面印上彩色或黑色的广告，背面则加进一些新的东西——孩子的着色游戏、主妇的美味食谱或全家一起玩的游戏。有一位丈夫抱怨洗衣店的费用激增，他发现妻子竟然为了收集乔治的食谱，把可以再穿一天的衬衣送洗。亿万富翁亨利?福特说，“思考是世上最艰苦的工作，所以很少人愿意从事它。”
      
【温馨提示】

      成功学大师拿破仑?希尔说，如果你想变富，你需要思考，独立思考而不是盲从他人。富人最大的一项资产就是他们的思考方式与别人不同。成功有捷径吗？拿破仑?希尔的回答是肯定的。致富的捷径就是以积极的思考致富，相信你能，你就做得到，不论你是谁，不论年龄大小，不论学问程度高低，都能招揽财富。“你的头脑就是你最有用的资产。”成功者从不墨守成规，而是积极思考，千方百计来对方法和措施予以创造性的改进。如果你一味地只做别人做的事，你最终只会拥有别人拥有的东西。学会思考吧，每一天1440分钟，哪怕你用1%的时间来思考、研究、规划，也一定会有意想不到的结果出现。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "通往天堂之路",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一位初到小镇的神父问一个小孩“小朋友，你可以告诉我怎样可以去邮政局吗？”小孩详细地告诉了神父。
      神父说：“小朋友，非常谢谢你。对了，星期天你来教堂找我吧，我可以告诉你通往天堂的路”。
      小孩说：“算了吧，你连到邮政局的路也不懂，又怎能告诉我通往天堂的路呢?”
      
【温馨提示】

      如果我们要获得别人的信任，我们必须先要充实自己。同样，作为父母，连自己都消极对待人生了，怎能让孩子去积极面对生活和学习呢？",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "专注与单纯——获得人生智慧",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一、单纯
      英国某家报纸曾举办一项高额奖金的有奖征答活动。题目是：在一个充气不足的热气球上，载着3位关系人类兴亡的科学家。第一位是环保专家，他的研究可拯救无数人免于因环境污染而面临死亡的噩运。第二位是原子专家，他有能力防止全球性的原子战争，使地球免于遭受灭亡的绝境。第三位是粮食专家，他能在不毛之地运用专业知识成功地种植谷物，使几千万人脱离因饥荒而亡的命运。此刻热气球即将坠毁，必须丢出一个人以减轻载重，使其余2人得以生存。请问，该丢下哪一位科学家？问题刊出后，因为奖金的数额相当庞大，各地答复的信件如雪片飞来。在这些答复的信中，每个人皆竭尽所能，甚至天马行空地阐述他们认为必须丢下哪位科学家的见解。最后结果揭晓，巨额奖金得主是一个小男孩。他的答案是——将最胖的那位科学家丢出去。
      
【温馨提示】

      单纯的思考方式，往往比钻牛角尖更能获得良好的成功。任何疑难问题的最好的解决方法，只有一种，就是能真正切合该问题所需求的，而非惑于问题本身的盲目探讨。二、专注一位农场主巡视谷仓时，不慎将—只名贵的手表遗失在谷仓里。他遍寻不获，便定下赏价，承诺：谁能找到手表，就给他50美元。人们在重赏之下，都卖力地四处翻找，可是谷仓内到处都是成堆的谷粒，要在这当中找寻—只小小的手表，谈何容易。许多人一直忙到太阳下山，仍一无所获，只好放弃了50美元的诱惑而回家了。仓库里只剩下一个贫困的小孩，仍不死心，希望能在天完全黑下来之前找到它，以换得赏金。谷仓中慢慢变得漆黑，小孩虽然害怕，仍不愿放弃，不停地摸索着，突然他发现在人声安静下来之后，有一个奇特的声音。那声音滴答、滴答不停地响着，小孩顿时停下所有的动作，谷仓内更安静了，滴答声也变得十分清晰，是手表的声音。终于，小孩循着声音，在漆黑的大谷仓中找到了那只名贵的手表。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "上帝在拍照",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      电闪雷鸣的一天，一位妈妈担心放学的女儿害怕闪电，就急急开车去接宝贝女儿，当她看见雨中的女儿时，她小脸上没有害怕的神情，还时不时地露出天真浪漫的笑容，妈妈把小女孩迎上车，担心地问道：“宝贝，你不害怕吗？”女孩天真无邪的回答：“干嘛害怕呀？这是上帝在拍照，我已经被拍了好多张呢！”
      
【温馨提示】

      孩子的思想是何其简单，而简单又是何其快乐！",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "换位思考——蹲下再说",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      妈妈牵着孩子走在圣诞前夕的街上，商场、大街处处张灯结彩热闹非凡。细心的妈妈发现孩子并不开心，节日气氛并没有感染到孩子。妈妈蹲下身来对孩子说：“瞧，多漂亮呀，可宝贝，你为什么不开心......”妈妈还没说完，却已明白孩子不开心的原因了，因为孩子矮小，从他的高度看去，大人们的PP挡住了美丽的景色。随后，妈妈高高地把孩子抱起来，孩子那张可爱的小脸很快泛出了笑容“妈妈快瞧那儿，那棵圣诞树太漂亮了......”
      记住，孩子眼里的世界和大人眼里的世界的是不同的。
      
【温馨提示】

      父母教育孩子时，要从孩子的角度去看问题，换位思考相当重要，孩子如果能处处理解父母的教诲那他（她）就不是孩子了。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "勇士和谋士",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      勇士和谋士一同赶路。
      来到一条激流前，谋士说：“江水太急，渡江恐有危险，还是找一找是否有一座桥可以过江吧。”勇士说：“怕什么，水火无畏。”于是，勇士在湍急的江水里奋搏，历经风浪，渡到对岸。发现谋士也已顺利过江，于是他们又一同赶路。
      来到草地边，突然下起大雨，谋士说：“咱们避一避雨，等天晴了绕开草地走吧。”勇士说：“怕什么，风雨无阻。”于是，勇士冒着风雨横穿草地，在沼泽地里几经挣扎，差一点没命。雨过天晴，勇士和谋士又遇到了一块，于是他们又一同赶路。
      来到森林旁，谋士说：“森林里恐有豺狼虎豹，咱们还是绕道走吧。”勇士说：“怕什么，人定胜天。”于是，勇士独自穿越森林，一路斩巨蟒、杀豺狼，终因体力耗尽，死于虎口。
      比尔?盖茨在读大二的时候，曾邀他的一位同学科莱特一起退学，去开发32Bit财务软件。科莱特认为，虽然新编的教科书已经解决了进位制路径转换问题，但仍还有许多难题有待攻克。他于是委婉拒绝了比尔?盖茨，继续念完大学，又成了计算机Bit方面的博士研究生。10年后，他攻克了无数Bit方面的难题，成了Bit专家。
      比尔?盖茨却退了学，创办了微软公司。在开发32Bit财务软件上，他遇到了许多困难，最后，他毅然绕过Bit系统，继而开发出Eip财务软件，而且其速度比Bit要快1500倍。10年后，比尔?盖茨成为全球首富。
      科莱特不失为一位勇士，但比尔?盖茨却是家喻户晓的成功者。
      
【温馨提示】

      很多人认为，不怕困难、无所畏惧，才是勇敢者。勇敢固然可敬，但如果我们避开困难，绕道走，也许更容易成功。",
            group9));

            group9.Items.Add(new SampleDataItem("Group-4-Item-3",
            "狮子和熊",
            "Item Subtitle: 3",
            "Assets/title/9.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      狮子和熊在森林里走着，肚子非常饿，他们很懊恼，一路走一路吵，大象见他们这对好朋友闹矛盾了，就想问问究竟，他们俩委屈地说出了事情的原委。
      狮子和熊去寻找猎物。走了好半天，目光敏锐的狮子一下子发现了山坡上有只小鹿，狮子正要扑上去，熊一把拉住他说：“别急，鹿跑得快，我们只有前后夹击，才能抓住它。”狮子听了，觉得很有道理，就分头行动了。
      鹿正津津有味地啃着青草，忽然听到背后有响声。他回头一看：哎呀，不得了！一只狮子轻手轻脚向他扑过来了！鹿吓得撒腿就跑，狮子在后面紧追不舍，无奈，鹿跑得真快，狮子追不上。这时，熊从旁边窜出来，挡住鹿的去路。他挥着蒲扇大的巴掌，一下子就把鹿打晕了过去。狮子随后赶到，他问到：“熊老弟，猎物该怎么分呢？”熊回答说：“狮大哥，那可不能含糊，谁的功劳大，谁就分得多。”狮子说：“我的功劳大，鹿是我先发现的。”熊也不甘示弱：“发现有什么用，要不是我的主意，你能抓到吗？”狮子很不服气地说：“如果我不把鹿赶到你这里，你也抓不到呀！”两人你一言我一语争个不休，谁也不让谁，都认为自己的功劳大，说着说着就打了起来。
      被打晕的鹿渐渐醒了过来，看到狮子和熊打得不可开交，赶紧爬起来，一溜烟逃走了。当他们打得精疲力竭时，回头一看，鹿早不见了。
      大象了解事情的经过后，平心静气地说：“你们能一起抓获鹿，却在最后时刻闹这样的矛盾，实在不值得，如果你们一直坚持同心合作，共享成果，现在就不用饿肚子了。”
      
【温馨提示】

      生命中有许多重要时刻，往往需要与别人互相信任的团结合作。只有这样，才有可能迅速地解决问题度过难关，享受丰硕的成果。",
            group9));



            this.AllGroups.Add(group9);
            #endregion

            #region 第10章
            var group10 = new SampleDataGroup("Group-10",
            "第10章",
            "Group Subtitle: 5",
            "Assets/title/10.png",
            "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
"再试一次就有奇迹",
"Item Subtitle: 3",
"Assets/title/10.png",
"Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
@"
      1943年，美国的《黑人文摘》刚开始创刊时，前景并不被看好。它的创办人约翰逊为了扩大该杂志的发行量，积极地准备做一些宣传。他决定组织撰写一系列“假如我是黑人”的文章，请白人把自己放在黑人的地位上，严肃地看待这个问题。他想，如果能请罗斯福总统夫人埃莉诺来写这样的一篇文章最好不过了。于是约翰逊便给她写去了一封非常诚恳的信。
      罗斯福夫人回信说她太忙，没时间写。但是约翰逊并没的因此而气馁，他又给她写去了一封信，但她回信还是说她很忙。以后，每隔半个月，约翰逊就会准时给罗斯福夫人写去一封信，言辞也愈加恳切。不久，罗斯福夫人因公事来到约翰逊所在的芝加哥市，并准备在该市逗留两日。约翰逊得此消息，喜出望外，立即给总统夫人发了一份电报，恳请她趁在芝加哥逗留的时间里，给《黑人文摘》写那样一篇文章。罗斯福夫人收到电报后，没有再拒绝。好觉得，无论多忙，她再也不能说“不”了。
      这个消息一传出去，全国都知道了。直接的结果是，《黑人文摘》杂志在一个月内，由2万份增加到了15万份。后来，他又出版了黑人系列杂志，并开始经营书籍出版、广播电台、妇女化妆品等事业，终于成为闻名全球的富豪。
      
【温馨提示】

      成功从来就不会是一条风和是丽的坦途，面对每一次挫折与失败，我们应该始终怀有“再试一次”的勇气与信心。也许再试一次，我们就听见了成功的脚步声！",
group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "迪斯尼乐园的人性设计",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      世界著名建筑大师格罗培斯设计的迪斯尼乐园，经过了3年的施工，马上就要对外开放了。然而各景点之间的道路该怎样联络还没有具体的方案。施工部打电话给正在法国参加庆典的格罗培斯大师，请他赶快定稿，以便按计划竣工和开放。
      格罗培斯大师从事建筑研究40多年，攻克过无数建筑方面的难题，在世界各地留下了70多处精美的杰作。然而建筑中最微不足道的一点小事──路径设计却让他大伤脑筋。对迪斯尼乐园各景点之间的道路安排，他已修改了50多次，没有一次是让他满意的。接到催促电报，他心里更加焦躁。巴黎的庆典一结束，他就让司机驾车带他去了地中海海滨。他想清醒一下，争取在回国前把方案定下来。汽车在法国南部的乡间公路上奔驰，这里是法国著名的葡萄产区，漫山遍野到处是当地农民的葡萄园。一路上他看到人们将无数的葡萄摘下来提到路，向过往的车辆和行人吆喝，然而很少有人停下来。当他们的车子进入一个小山谷时，发现在那里停着许多车子。原来这儿是一个无人看管的葡萄园，你只要在路边的箱子里投入5法郎就可以摘一篮葡萄上路。据说这座葡萄园主是一位老太太，她因年迈无力料理而想出这个办法。起初她还担心这种办法能否卖出葡萄，谁知在这绵延百里的葡萄产区，她的葡萄总是最先卖完。她这种给人任其选择的做法使大师格罗培斯深受启发，他下车摘了一篮葡萄，就让司机调转车头，立即返回了巴黎。回到住地，他给施工部发了一封电报：撒上草种提前开放。
      施工部按要求在乐园撒了草种，没多久，小草出来了，整个乐园的空地都被绿草覆盖。在迪斯尼乐园提前开放的半年里，草地被踩出许多小道，这些踩出的小道有窄有宽，优雅自然。第二年，格罗培斯让人按这些踩出的痕迹铺设了人行道。1971年，在伦敦国际园林建筑艺术研讨会上，迪斯尼乐园的路径设计被评为世界最佳设计。当人们问他，为什么会采取这样的方式设计迪斯尼乐园的道路时，格罗培斯说了一句话：艺术是人性化的最高体现。最人性的，就是最好的。
      
【温馨提示】

      让太阳像太阳那样升起，让星星像星星那样闪烁，让树像树一样成长，让人像人一样生活──这就是最人性的。“最人性的”方式就是对生命和万物最尊重的方式。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "室外电梯是清洁工的创意",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      柯特大饭店是美国加州圣地亚哥市的一家老牌饭店，由于原先配套设计的电梯过于狭小老旧，已无法适应越来越多的客流。于是，饭店老板准备改建一个新式的电梯。他重金请来全国一流的建筑师和工程师，请他们一起商讨，该如何进行改建。建筑师和工程师的经验都很丰富，他们讨论的结论是：饭店必须新换台大电梯。为了安装好新电梯，饭店必须停止营业半年时间。
      “除了关闭饭店半年就没有别的办法了吗?”老板的眉头皱得很紧，“要知道，那样会造成很大的经济损失……”
      “必须得这样，不可能有别的方案。”建筑师和工程师们坚持说。
      就在这时候，饭店里的清洁工刚好在附近拖地，听到了他们的谈话。他马上直起腰，停止了工作。他望望忧心忡忡神色犹豫的老板和那两位一脸自信的专家，突然开口说：“如果换上我，你们知道我会怎么来装这个电梯吗？”工程师瞟了他一眼．不屑地说：“你能怎么做？”“我会直接在屋子外面装上电梯。”工程师和建筑师听了，顿时诧异得说不出话来。
      很快，这家饭店就在屋外装设了一部新电梯。在建筑史上，这是第一次把电梯安装在室外。
      
【温馨提示】

      不要因为别人都这样做事，我们也一定要这样做事；不要因为过去是这样做，现在就得这样做。换一种思路，甚至用完全相反的方法试一下，你会发现问题同样得到解决，但结果可能完全不同。当别人都纵向地将苹果切开，你不妨横着切一次，你会发现苹果里原来还隐藏着那么多美丽的图画。创意是了无痕迹的匠心。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "期望+幽默+热情＝？",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有位先生走进西部航空公司的售票厅，对售票小姐说：“我想买到打折机票，你们有打折票吗？两张，到旧金山。”
      “当然有。不过，请问，您是美国印弟安人吗？”
      “不是。你问这干吗？”
      “那太遗憾了，先生，如果您是印第安人，并在清晨４点启程，又在次日清晨返回的话，我们可以给您30％的减价优惠。”
      “哎，我的上帝，请问你们还有其他优惠条件吗？”
      “有呀。如果你已结婚20年以上没有离婚，并且是参加您的结婚纪念活动，我们给您减价20％。”
      “这对我不合适，还有吗？”
      “哎呀，您太太是射手座吗？如果她是射手座，而且你们又不赶在周末旅行，那么可能享受25％的优惠价。”
      “可是我们非得在周末才有空呀，况且她不是射手座。”
      “别灰心。请问，您和您夫人中谁还是学生吗？如果你们有一个在上大学，而且又是在星期五乘飞机，我们给您45％的减价优惠。”
      “我的天，差不多便宜一半啊！可惜我们早几年就念完大学了。这样吧，小姐，谢谢您的耐心介绍。您还是不要给我优惠了，随便两张什么票都可以。”
      于是，他心满意足地买下了两张全额机票。
      
【温馨提示】

      让你拥有一份美好的期望，再给这份期望加进一点幽默，在这种幽默里，再加上真诚的热情……其实有些时候，我们对精神优惠的需要远比对金钱优惠的需要更强烈。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "一美元的轿车",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      美国的一家报纸登了这么一则广告：“一辆崭新轿车，售价l美元。”哈利看到这则广告时半信半疑：“今天不是愚人节啊！”但是，他还是揣着l美元，按照报纸上提供的地址找了过去。
      在一栋非常漂亮的别墅前，哈利敲开了大门。接待他的是一位服饰华贵的少妇，问明来意后，少妇把哈利领到车库前，指着一辆崭新的豪华轿车说；“喏，就是它。”哈利脑子里闪过的第一个念头就是：“是坏车。”他说：“太太，我可以试试车吗？”“当然可以!”于是哈利开着车兜了一圈，一切正常。“这辆轿车不是赃物吧？”哈利要求验看车照，少妇拿给他看了。于是哈利付了1美元。当他开车要离开的时候，仍百思不得其解他说：“太太，您能告诉我这是为什么吗？”少妇叹了一口气：“实话跟您说吧，这是我丈夫的遗物。他把所有的遗产都留给了我，而这辆轿车，他答应给他的情妇，一个三流女演员。但是，他在遗嘱里把这辆车的拍卖权交给了我，由我将所卖款项交给他的情人。于是，我决定以1美元的价格卖掉它。”
      哈利开着轿车高高兴兴地回家了。路上，他碰到老朋友汤姆，汤姆好奇地问起轿车的来历，哈利眉飞色舞地将过程讲述一遍。没等他说完，汤姆就懊悔得捶胸顿足：“天啊，一周前我就看到这则广告了！可是我怎么也不相信。”
      
【温馨提示】

      请记住：在这个世界上，什么事都有可能发生。那些连奇迹都不敢相信的人，又怎么能获得和创造奇迹呢？",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "叫我第一名",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      每个孩子都是上帝给父母，乃至整个家庭和社会的礼物；因为我们都是自己人生的第一名！
      我们从何而来？我们是爸爸体内上亿颗小蝌蚪（精子）中的游泳冠军，和妈妈体内无数美女卵子中最优雅、知性、美貌、温柔（总之是万千宠爱与一生）的大美女结合的产物；任何一个稍稍落后的小蝌蚪和美女都无法造就如此完美的我们！
      也许你会说：我不是最漂亮的，但你绝对是最善解人意的！
      也许你会说：我不是最聪明的，但你绝对是最孝敬长辈的！
      也许你会说：我不是最温柔的，但你绝对是最才华横溢的！
      也许你会说：我身体还有些残缺，那是因为你就是上帝派来人间的天使！
      也许你会说：我还有很多坏习惯，但你绝对可以战胜自己成为更好的你！
      上帝给我们每个人一点点小缺点，让我们来到人间有事可做；那就是弥补自己的不足，成为人生永远的第一名！
      第一名不在于成绩，而在于积极地人生态度；
      第一名不在于容貌，而在于温暖柔软的心底；
      第一名不在于成就，而在于幸福却又平凡的生活！
      我们都是自己出生的第一名，我们也该成为自己人生的第一名！
      请叫我第一名！
      
【温馨提示】

      请记住：在这个世界上，什么事都有可能发生。那些连奇迹都不敢相信的人，又怎么能获得和创造奇迹呢？",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "天生我材必有用不可以貌取人",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      还记得那个路人歌手保罗帕兹吧，他在电视节目『英国达人』（Britain'sGotTalent）中一唱惊人，一夕之间从路人变名人，还巡回好几个国家演唱，足足为他攒进600万英镑的财富，呵呵呵，照过来照过来，上礼拜六『英国达人』又出现「不世路人」好歌喉了啦，甚至连黛咪摩尔都听得留下泪珠来呢！
      刚开始，47岁的苏珊波伊尔（SusanBoyle）一走上台，根本没人想甩她，从头土到脚不说，台下观众还一副想看她笑话的表情，就别提超尖酸刻薄的评审赛门柯威尔，不停问话嘲讽她想让她出糗，甚至表明她住的乡下地方，连听都没听过呢，不过，苏珊没被吓到，小小眼珠闪出Bling的光芒，透露出刘佬佬进大观园的新鲜神情。
      接着就是她的惊天一唱，当苏珊才开口唱出舞台剧「悲惨世界」的名曲「Idreamedadream」，全场只能用欢声雷动、从头尖叫鼓掌到尾来形容，许多电视机前面的妇女同胞们，还不断打电话进电视台，说自己边看边哭了呢，其中也包括好莱坞大明星黛咪摩尔。
      她说自己礼拜六晚上本来是被老公艾希顿库奇强迫去看这个节目，但当一听见苏珊的歌声之后，眼泪就一滴一滴不能自己地留下来，叫她好不感动，目前苏珊波伊尔的演唱影片，已经在YouTube快速发烧，从一个晚上1.3百万人次浏览，到现在将近四百万人次，都已经快超越保罗帕兹的纪录囉。
      倒是苏珊，当媒体蜂拥而至她住的小村庄访问时，她还搞不清楚状况，直说完全不了『英国达人』全世界都看得到，挺吓人的，因为她原本只是想上电视曝光，看有没有机会找个伴，因为活了47年的她，从来没谈恋爱接吻过。
      自从二年前她母亲过世之后，她更是不唱歌不出门地躲在家自哀自怜，现在却暴红，除了评审兼制作人赛门要给她一纸唱片合约之外，美国和澳洲的电视台还频频跟她连络邀她上节目，简直叫她不敢置信。
      不过，苏珊没让名利给冲昏头，知道绚烂终会归于平静，她现在还是希望能找到一个伴。
      
【温馨提示】

      请记住：在这个世界上，什么事都有可能发生。那些连奇迹都不敢相信的人，又怎么能获得和创造奇迹呢？",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "母狼的启示",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个朋友的孩子大学毕业半年了，没有去找事，窝在家里，白天睡觉，晚上上网。最近跟他父母要钱，想去美国游学，朋友来问我该不该让他去，我望着他苍苍的白发说：「你如果真的要为孩子好，让他去，但是不要给他钱。」
      我想到了我妹婿的故事。我妹婿是美国人，从小就想作水手，向往外面的世界，想先环游世界再回学校念。虽然他父亲是医生，家庭经济环境许可，但是父母并不给他钱，他也没向家里要。高中一毕业就先去阿拉斯加伐木存钱。因为阿拉斯加夏天日照很长，太阳到午夜才落下，三点多又升上来了，他一天如果工作十六小时，伐一季木的工资可以让他环游世界三季。他在走遍世界两年之后才回大学去念书。因为他是在自己深思熟虑之下才决定念的科系，所以三年就把四年的学分修完，出来就业。
      他工作得很顺利，可以说平步青云，一直做到总工程师。有一次，他告诉我一个小故事，说这件事影响了他一生。他在阿拉斯加打工时，曾与一个朋友在山上听到狼的嗥叫声，他们很紧张的四处搜寻，结果发现是一只母狼脚被捕兽器夹住，正在号嚎，他一看到那个奇特的捕兽器，就知道是一名老工人的，他业余捕兽，卖毛皮补贴家用，但是这名老人因心脏病已被直升机送到安克瑞契医院去急救了，这只母狼会因为没有人处理而饿死。他想释放母狼，但母狼很凶，他无法靠近，他又发现母狼在滴乳，表示狼穴中还有小狼，所以他与同伴费了九牛二虎之力找到狼穴，将四只小狼抱来母狼处吃奶，以免饿死。他把自己的食物分给母狼吃，以维持母狼的生命，晚上还得在母狼附近露营，保护这个狼家庭，因为母狼被夹住了，无法自卫。
      一直到第五天，他去喂食时，发现母狼的尾巴有稍微摇一摇，他知道他已开始获得母狼的信任了，又过了三天，母狼才让他靠近到可以把兽夹松开，把母狼释放出来。母狼自由后，舐了他的手，让他替牠的脚上药后，才带着小狼走开，一路还频频回头望他。
      他坐在大石头上想，如果人类可以让凶猛的野狼来舐他的手，成为朋友，难道人类不能让另一个人放下武器成为朋友吗？他决定以后先对别人表现诚意，因为从这件事中看到，先释放出诚意，对方一定会以诚相报。（他开玩笑说，如果不是这样，那就是禽兽不如。）因此，他在公司中以诚待人，先假设别人都是善意，再解释他的行为，常常帮助别人，不计较小事。所以他每年都升一级，爬得很快。最重要的是，他每天过得很愉快，助人的人是比被助的人快乐得多，虽然他并不知道圣经中有「施比受更有福」这句话，但是他的生活证明了这一点。
      他对我说，他一直很感谢阿拉斯加的经验，因为这使他一生受用不尽。
      的确，只有自己想要的东西才会珍惜，下过霜的柿子才会甜，人也是经过磨鍊了才会成熟。
      领悟到[对孩子最好的保护就是不保护]如果一个人大学毕业了还不知道自己要什么，那么应该要让他去外面磨鍊一下，不要给他钱，让他自食其力，重要的是父母要舍得放下！给他一个机会去证明自己、体验人生，相信他也能从中得到一个对他一生受用不尽的经验。
      「如果不能使工作少一点，就让快乐多一点」
      
【温馨提示】

      请记住：在这个世界上，什么事都有可能发生。那些连奇迹都不敢相信的人，又怎么能获得和创造奇迹呢？",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "家不是讲理的地方",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      家不是讲理的地方——一份不在婚礼上的家长致辞
      萍儿：
      爸妈听到你要结婚喜讯，真为你高兴，远隔千里，我们不能参加你的婚礼，不能在婚礼上献上我们的家长致辞，这是我们内心的一点遗憾．但你是我们的女儿，相信你能理解爸妈的一份感受。
      那天放下电话，我和你妈沉默了良久－－我们的孩子怎么一下子就长大了？后来，你妈对着镜子淡淡地说：你看，我的白头发又多了，萍儿也该结婚了。我数着你妈的白发，那些白发竟像许多往事，一件一件浮上心头．你妈提醒我：还是给女儿写一封信吧！就算是我们送你的一份新婚礼物。
      萍儿，我们只是天底下最平凡的父母，我们的孩子也只是天底下最平凡的女儿，我们不奢求太多，只是希望我的孩子踏上婚姻之路，走向人生之旅后，能满怀感恩，一路平安在这里，给你一个我们的生活体会。
      先要告诉你：家不是一个讲理的地方。
      这句话听起来，很没有道理，但千真万确，这句话是真理；是至理，是多少夫妇；多少家庭（包括我们家），用多少岁月；多少辛酸；多少爱恨；多少是非；多少对错，在纠缠不清难解难分的混乱中，梳理出来的一个最后结论。
      当夫妇之间开始据理力争时，家里便开始布上阴影。两人都会不自觉地各抱一堆面目全非的歪理，敌视对方；伤害对方；最后只能两败俱伤，难以收拾。
      多少夫妻，为了表面的一个〔理〕，落得负心无情。他们不知道，家不是讲理的地方，不是算账的地方。
      那么；何为〔家〕，家是什么地方？
      萍儿！
      我们年轻的时候；也回答不了这个问题，也像许多夫妻那样，为一点小事争闹不休。那一年为了你小叔的调动问题，我和你妈大吵了一场，甚至闹到要离婚的地步。只是在那个时代我们还缺乏勇气．直到有一天，一位老战友在他孩子的婚礼上说：〔希望你白头偕老，相爱永远〕时，爱这简短的字，像春雷响在我心里。
      是的，家不是讲理的地方，家该是讲爱的地方。
      爱一时很容易，爱一生一世却不容易，这里面有许多妙处需要我们去总结和体会。
      其次我们要告诉你：婚姻是个空盒子，你必须往里面放东西；才能取回你要的东西，你放的愈多；得到的也就愈多。
      很多人结婚时，对婚姻有许多期盼，期盼从中可以得到富贵、藉慰、爱情、宁静、快乐、健康，其实婚姻开始的时候，只是一个空盒子。走到一起的两个人，一定要养成一个习惯，去给，去爱，彼此侍奉，彼此赞赏，日后，那个空盒子才会日渐丰富起来。
      空盒子最先该放的应该是〔思念〕，思念是一种使我们刻骨铭心的东西。它是两个人有了肯定，有了情感，然后进而关怀，进而疼爱的一种情绪。〔思念〕是疲惫时通向家的一条小路，是寒冷冬夜里的一股暖意，是匆忙推开家门后扑面而来家饭香....
      空盒子里还要放进〔艺术〕，婚姻生活中的艺术。在婚姻生活中，需要讲艺术的地方无处不在，生气有艺术，吵架有艺术。
      有一对夫妇含辛茹苦养育了五个孩子，其中之艰难，只有他们自己能体会。一天，夫妻为了孩子的一件小事吵起来，越吵越厉害，眼看不可收拾了，妻子突然说：等一下，我要去生孩子了。这句话，就是吵架的艺术。
      婚姻的盒子里，除了放〔思念〕和〔艺术〕外，还有许多东西，都可以放进去，这有待于你们自己去填补。
      写到这里想到一位作家说过的一句话：〔你们生养他，教育他，你们的责任已尽，而你们给他最好的礼物，是一对翅膀〕。
      萍儿：这封信，就是爸妈送给你的结婚礼物，我们希望你带着我们的祝福，快乐的飞翔！
      爱一个人……
      要了解；也要开解，
      要道歉；也要道谢，
      要认错；也要改错，
      要体贴；也要体谅，
      是接受；而不是忍受，
      是宽容；而不是纵容，
      是支持；而不是支配，
      是慰问；而不是质问，
      是倾诉；而不是控诉，
      是难忘；而不是遗忘，
      是彼此交流；而不是凡事交代，
      是为对方默默祈求；而不是向对方诸多要求。
      
【温馨提示】

      请记住：在这个世界上，什么事都有可能发生。那些连奇迹都不敢相信的人，又怎么能获得和创造奇迹呢？",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "高速公路上的“玛卡若线”",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      或许你从来没有听说过一个叫琼?玛卡若医生的名字，但这个世界注定要留下她的痕迹。
      琼出生在美国内布拉斯加州，出于一个医生的敏感和良知，因车祸造成的一个个患者让她对高速公路的安全问题产生了极大的关注。她决定为减少车祸做些有益的事情。行驶在高速公路上，琼注意到，人们都乐意在道路中间的部分行驶，这种偏好使车辆相撞的几率大为增加。一个念头闪现在琼的脑海中：在公路中间画上一条醒目的线。当琼将她的建议提交给有关部门时，得到的却是冷酷的否定。不过，琼不是一个轻言放弃的人，她又将她的设想提供给一个妇女俱乐部，俱乐部所有成员全票赞成这一提议。尽管有妇女俱乐部的鼎力支持，琼还是在经过了７年不屈不挠的奔走之后，才终于使自己的提议得到实施。
      1924年，内布拉斯加州高速公路管理委员同意在99号高速公路进行实验，他们在这条公路的中间划上了醒目的一条线，将路面平分为两部分。此后的统计数字表明，99号高速公路的事故率大幅度地下降了。不久，该州的高速公路都划上了中分线。随后，世界上的绝大多数国家都采用了这种做法。
      作为医生的琼没有在医学界留下自己的影子，却在世界交通史上永远刻下了自己名字──为了纪念她，人们将马路中间的这条线叫做“玛卡若线”。
      
【温馨提示】

      如果你对这个世界怀有深切的关爱，你就会设法为完善这个世界做一些事情。出于这样的动机，爱迪生发明了电灯，弗莱明发明了青霉素，杜瓦发明了保温瓶，玛卡若发明了“玛卡若线”......",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "富翁的秘密",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      哈里出生在一个嘈杂的贫民窟里，和所有贫民窟的孩子一样，他经常打斗、喝酒、吹牛和逃学，唯一不同的是：他天生有一种赚钱的眼光。他把从街上捡来的一辆破玩具车修整好，然后租给同伴们玩，每人每天收取半美分租金。一个星期之内，他竟然赚回了一辆新玩具车。他的老师对他说：“如果你出生在富人家庭，你会成为一个出色的商人。但是，这对你来说不可能。不过，也许你能成为街头的一位商贩。”
      中学毕业后，正如他的老师所说他真的成了一个商贩。他卖过小五金、电池、柠檬水，每一样都得心应手。最后让他发迹的是一堆丝绸服装。这些服装来自日本，因为在海上遭遇风暴，染料浸染了丝绸，数量足足有一吨之多，结果一船的货都变成了废品，成了日本商人头疼的东西。他们想低价处理掉，却无人问津，想搬运到港口扔进垃圾堆，又怕被环保部门处罚。于是，日本人打算在回程的路上把丝绸抛到海中。有一天，哈里在港口的一个地下酒吧喝酒，当他步履蹒跚地走过一位日本海员旁边时，正好听到有人在谈论丝绸的事情。第二天，他就来到了海轮上，用手指着停在港口的一辆卡车对船长说：“我可以帮忙把丝绸处理掉，如果你们愿意象征性地给一点运费的话。”他不花任何代价拥有了这些被雨水浸过的丝绸。他把这些丝绸加工成迷彩服、领带和帽子，拿到人群集中的闹市出售。几天之间，他靠这些丝绸净赚了l0万美元。现在哈里已不是商贩，而是一个商人了。有一次他在郊外看上了一块地，就找到土地的主人，说他愿花10万美元买下来。主人拿了他的10万美元，心里嘲笑他的愚蠢，这样一个偏僻的地段，只有傻子才会出这个价。一年后，市政府对外宣布，要在郊外建造环城公路，哈里的地皮一下子升值了150多倍。从此，他成了远近闻名的富翁。
      在哈里77岁时，终于因病不能进行任何商务活动。然而，就在临死前，他让秘书在报纸上发布了一则消息，说他即将要去天堂，愿意为人们向已经去世的亲人带一个祝福的口信，每则收费100美元。结果他又赚了10万美元。如果他能在病床上多坚持几天，可能还会赚得更多一些。他的遗嘱也十分特别，他让秘书再登一则广告，说他是一位礼貌的绅士，愿意和一位有教养的女士同卧一块墓穴。结果，一位贵妇人愿意出资5万美元和他一起长眠。
      有一位资深的经济记者．热情洋溢地报道了他生命最后时刻的经商经历，文中感叹道：“每年去世的富人难以数计，但像他这样怀着对商业的执著精神坚持到最后的人能有几个？”这就是哈里怎样成为千万富翁的全部秘密。
      
【温馨提示】

      机会无处不在、无时不在，关键是你如何能把握它。你不认识机会，机会就永远不会认识你。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "石油大王洛克菲勒",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一位年轻人，在一家石油公司里谋到一份工作，任务是检查石油罐盖焊接好没有。这是公司里最简单枯燥的工作，凡是有出息的人都不愿意干这件事。这位年轻人也觉得，天天看一个个铁盖太没有意思了。他找到主管，要求调换工作。可是主管说：“不行，别的工作你干不好。”年轻人只好回到焊接机旁，继续检查那些油罐盖上的焊接圈。既然好工作轮不到自己，那就先把这份枯燥无味的工作做好吧！从此，年轻人静下心来，仔细观察焊接的全过程。他发现，焊接好一个石油罐盖，共用39滴焊接剂。为什么一定要用39滴呢？少用一滴行不行？在这位年轻人以前，已经有许多人干过这份工作，从来没有人想过这个问题。这个年轻人不但想了，而且认真测算试验。结果发现，焊接好一个石油罐盖，只需38滴焊接剂就足够了。年轻人在最没有机会施展才华的工作上，找到了用武之地。他非常兴奋，立刻为节省一滴焊接剂而开始努力工作。原有的自动焊接机，是为每罐消耗39滴焊接剂专门设计的，用旧的焊接机，无法实现每罐减少一滴焊接剂的目标。年轻人决定另起炉灶——研制新的焊接机。经过无数次尝试，他终于研制成功了“38滴型”焊接机。使用这种新型焊接机，每焊接一个罐盖可节省一滴焊接剂。积少成多，一年下来，这位年轻人竟为公司节省开支5万美元。一个每年能创造5万美元价值的人，谁还敢小瞧他呢？由此年轻人迈开了成功的第一步。许多年后，他成了世界石油大王──洛克菲勒。
      有人问洛克菲勒：“成功的秘决是什么？”他说：“重视每一件小事。我是从一滴焊接剂做起的，对我来说，点滴就是大海。”
      
【温馨提示】

      常人只看见别人拥有大海，却没看见别人一点一滴艰辛积累的过程。不寻常的人总会用不寻常的眼睛看待世界。点滴之水汇成江河，平凡小事筑就伟大。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "世界级汽车营销大王乔?吉拉德",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      有一次，一位中年妇女走进乔?吉拉德的展销室，说她想在这儿看看车，打发一会儿时间。她告诉乔?吉拉德，她想买一辆白色的福特车，就像她表姐开的那辆一样。但对面福特车行的推销员让她过一小时后再去，所以她就先来这儿看看。她说，这是她送给自己的生日礼物，“今天是我55岁生日。”“生日快乐！夫人。”乔?吉拉德一边说，一边把她让进办公室，自己出去打了一个电话。然后，乔?吉拉德继续和她交谈：“夫人，您喜欢白色车，既然您现在有时间，我给您介绍一下我们的双门式轿车──也是白色的。”他们正谈着，女秘书走了进来，递给乔?吉拉德一束玫瑰花。乔?吉拉德慎重地把花送给那位妇女：“尊敬的夫人，有幸知道今天是您的生日，送您一份薄礼，祝您好运！”她很受感动，眼眶都湿了。“已经很久没有人给我送礼物了。”她说，“刚才那位福特推销员一定是看我开了部旧车，以为我买不起新车，我刚要看车，他却说要去收一笔款，于是我就上这儿等他。其实我只是想要一辆白色车而已，只不过表姐的车是福特，所以我也想买福特。现在想想，不买福特也可以。”最后她在乔?吉拉德手里买走了一辆雪佛莱，并填了一张全额支票。其实从头到尾乔?吉拉德都没有劝她放弃福特而买雪佛莱。只是因为她在这里感觉受到了重视，于是放弃了原来的打算．转而选择了乔?吉拉德的产品。
      乔?吉拉德是世界级汽车营销大王，在15年的推销生涯中，共卖出13001辆汽车，曾创下了一年卖出1425辆（平均每天4辆）的纪录，这个成绩被列入《吉尼斯世界大全》。他的几万个客户，每隔一段时间就会接到他寄来的贺卡，上面只有这样的一些话：“祝你生日快乐”，“为你的荣升干杯”，“希望什么时候再能聆听你的教诲”……他的秘诀是：决不营销汽车，只营销问候。
      
【温馨提示】

      细节决定成败。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "哥伦布让鸡蛋站立",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      为了横越大西洋，哥伦布精心筹划了18年。其间，他受尽别人的嘲笑和奚落，被认为是愚蠢的梦想家。经过无数次辩论和游说，他的真诚和信念最后感动了西班牙国王和王后，他们给了哥伦布远航的船只。哥伦布成功地渡过了大西洋，并发现了美洲大陆。当哥伦布回到西班牙时，举国上下一片欢腾，人们对哥伦布充满了崇敬之情。国王和王后在宫廷里宴请他，异常兴奋地听他讲述航海过程中遇到的奇闻轶事。哥伦布的成功和荣耀引起了很多人的妒忌。他们说：“不就是一个因贫穷而做白日梦的穷水手吗？只要有足够大的船只，谁不能横渡大西洋呢？”听了别人的议论，哥伦布没有恼怒。他从容地站起来，对大家说：“如
      果你们有兴趣，我想提议在座的每一位做一个小小的游戏。很简单。看谁能把一个鸡蛋竖立起来。”每个人都尝试着把鸡蛋立起来，结果却失败了。最后大家一致认为，这是不可能办到的事情。这时，哥伦布顺手拿起一个鸡蛋，把尖端往桌面上轻轻一磕，鸡蛋就稳稳地立住了。哥伦布表情严肃地说：“各位，你们都说这件事情不可能办到，但我做到了。这是世界上最简单的事情，但等你们知道应该怎么做之后，谁都能做到了──关键在于谁先想到。”
      哥伦布把发现美洲的机会抢走了，当人们表示不满时，他把让鸡蛋立走来的机会给了大家，结果人们还是没有抓住。
      
【温馨提示】

      飞机让莱特兄弟发明了，相对论已经让爱因斯坦发现了，小说让海明威写了，电脑让比尔?盖茨做了……殊不知，我们的身边每时每刻都充满了创造奇迹的机会。“关键在于谁先想到”——要记住这句话，然后问：“我想了吗？我在怎么想？！”",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "本分与本事",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      商场经理检查新来的售货员一天的业务情况。
      “今天你向多少名顾客提供了服务？”经理问。
      “1名。”这名售货员答道。
      “仅仅1名顾客？”
      老板又问，“卖了多少钱？”
      售货员回答：“58,334美元。”
      经理大吃一惊，他请这位店员解释一下怎么卖了那么多钱。
      “首先我卖给了那个男人一只钓鱼钩，”售货员说，“接着卖给他一根钓竿和一只卷轴。然后我问他打算到什么地方钓鱼，他说去海里。所以我建议他应该拥有一条船──他就买了一艘20英尺长的小型汽艇。运走时，我带他到咱们商场的汽车销售部，卖给了他一辆微型货车。”
      老板惊愕不已地问道：“你真的卖了那么多东西给一个仅仅来买一只鱼钩的顾客？”
      “不！”新来的售货员回答：“他本来是到旁边柜台为他患偏头疼的夫人买一瓶阿斯匹林。我对他说：‘先生。你的夫人身体欠佳，周末如果有空，你不妨带着她去试试钓鱼，那真是太有意思了！’──事情就是这样。”
      
【温馨提示】

      尽多少本分，就得多少本事。如果你现在还不能十全十美、面面俱到地做好每件事，那就从做好一件事开始吧！",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "不要憎恨自己的敌人",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一位动物学家，对生活在非洲大草原奥兰治河两岸的羚羊群进行了长期研究。他发现，东岸羚羊群的繁殖能力比西岸的强，每分钟的奔跑速度也比西岸的快13米。这些莫名其妙的差别，曾使这位动物学家百思不得其解，因为这些羚羊的种类和生存环境都是相同的：都属羚羊类，都生长在半干旱的草原地带，饲料来源也一样，以一种叫莺萝的牧草为生。有一年，在动物保护协会的赞助下，动物学家在奥兰治河的东西两岸各捉了10只羚羊，分别把它们送往对岸。结果，从东岸运到西岸的10只羚羊，一年后繁殖到14只，从西岸运到东岸的10只，一年后还剩下3只，另外7只全被狮子吃了。这位动物学家终于明白，东岸的羚羊之所以强健，是因为它们附近生活着狮群；西岸的羚羊之所以弱小，正是因为缺少了天敌。
      没有天敌的动物往往最先灭绝，有天敌的动物反而会逐步繁衍壮大。大自然的这一悖论，在人类社会也同样存在。罗马曾经是一个强盛的帝国，但后来消失了。真正使罗马帝国灭亡的，不是别人，正是它自己──一个没有了敌手的帝国，终于在惰性的退化中消失。
      
【温馨提示】

      不要憎恨自己的敌人。真正促使一个人咬着牙坚持到底，真正激励一个人不断成功的，不是鲜花和掌声，不是亲人和朋友，而常常是那些可以置人于死地的打击和挫折，以及那些一直想将你彻底打败的敌人和对手。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "国际象棋的发明者西萨",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      古代印度的舍罕王，打算重赏国际象棋的发明者──宰相西萨。西萨向国王请求说：“陛下，我想向你要一点粮食，然后将它们分给贫困的百姓。”国王高兴地同意了。“请您派人在这张棋盘的第一个小格内放上一粒麦子，在第二格放两粒，第三格放四粒……照这样下去，每一格内的数量比前一格增加一倍。陛下啊，把这些摆满棋盘上所有64格的麦粒都赏赐给您的仆人吧！我只要这些就够了。”国王许诺了宰相这个看起来微不足道的请求。
      千百年后的今天。我们都知道事情的结局：国王无法实现自己的承诺。这是一个长达20位的天文数字！这样多的麦粒相当于全世界两千年的小麦产量。不过当时所有在场的人都不知道这个结果。他们眼看着仅用一小碗麦粒就填满了棋盘上十几个方格，禁不住笑了起来，连国王也认为西萨太傻了。随着放置麦粒的方格不断增多，搬运麦粒的工具也由碗换成盆，又由盆换成箩筐。即使到这个时候，大臣们还是笑声不断，甚至有人提议不必如此费事了，干脆装满一马车麦子给西萨就行了！不知从哪一刻起，喧闹的人们突然安静下来，大臣和国王都惊诧得张大了嘴：因为，即使倾全国所有，也填不满下一个格子了。
      
【温馨提示】

      弱小的事物当初总是被人讥笑，但只要不断积聚力量，就会逐渐强大。从弱变强的过程可能是难以察觉的，当你能够看见时，它就一定强大得令人难以置信。小事不做，大事难成。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "一匹马的命运",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      马多年独享一块肥沃的草地，后来有鹿也发现了这块草地。本来按马的食量，就是活一万年，也吃不完这块地上的草，但它却对鹿的闯人心存不快。于是，它想借助人的力量征服可恨的鹿．但狡猾的人却说：“我抓不到鹿，除非你让我骑着追上它。”马同意了，结果人骑着马追上了鹿。本来马和鹿的奔跑速度是人远不能及的，但为了报复鹿，马甘受其缚，结果它们都成了人的俘虏。直到这一刻，马才感到悔恨，但一切已无法改变。最终的胜家，不是跑得最快的马，也不是跑来分一杯羹的鹿，而是有智慧的人。
      
【温馨提示】

      逞一时之快，为了打击报复又不择手段，终会让自己付出沉重代价。马如此，人何尝不是如此？看别人不顺眼，其实是自己修养不够。一个人快乐，不是因为拥有的多，而是因为计较的少！",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "“吝啬专家”的学问",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      加拿大渥太华有两位“吝啬专家”，一位叫达希?珍，一位叫尼克森，他们都办了一份教人如何节俭过日子的报纸。
      达希?珍别号“狂热节俭家”，她自费出版了《安全守财奴月报》，多年来，向读者提供了无数省钱致富的秘诀。达希?珍说，赚钱渠道包括“找更高薪酬的职业”和“多省点钱”这两条路。她举了一个例子，一位部长级的官员虽有15万加元的年薪，但为了维持高官的面子，花在衣着、应酬、停车、保险、豪宅上面的钱占的份额，说不定会超过他的报酬，消费太高导致人不敷出。相反，过简单一点的日子，虽然赚的不多，反而能存下更多的钱。真正有钱的人不会住在最扎眼的高级社区，而常常住在普通公寓区；也不会开昂贵的豪华汽车，并且不到最后关头不会换车。更重要的是，有钱人都懂得节省和投资。达希?珍最后强调，你省下来的一块钱，大于你赚进的一块钱。
      尼克森在渥太华西郊的家里编辑出版《吝啬家月报》，传播勤俭致富的福音。每星期日，他主持CFRA电台的“省下来就是你的钱”节目与听众分享吝啬之道。尼克森在月报里提供了10项省钱致富的小秘诀：
      1、不断从收入当中拨出部分存款，5%、10%、25%都可以，反正一定要存；
      2、搞清楚你的钱每天、每周、每月用到哪里去了，要详细列一份预算与支出表；
      3、每次购物之后，要检查、核对所有的收据，看看商家有没有多收费；
      4、信用卡只需保留一张，能够证明身份就够了，欠账每月绝对付清；
      5、自备便当上班，这样每周可节省45加元午餐费，每年省下2200加元；付房子贷款或存作退休基金；
      6、与人共搭一辆车或乘大众交通工具上下班，节省停车费、汽油费、保险费、耗损费以及停车时间；
      7、多读些有关修理、投资、致富的《实用手册》，最好从图书馆借，或从网络下载，省钱；
      8、简化生活，房子不用太大，买二手汽车，到廉价商店、拍卖场、搬家大贱卖等地购物；
      9、买东西时切记“花这钱值不值得”，便宜货不见得划得来，贵也不保证品质就好；
      10、绝对要杀价，你不提出，店家绝不会主动减价卖给你东西。
      
【温馨提示】

      每个人在还没出生的时候就已经开始花钱了，所以，你必须重视金钱；金钱不是从天上掉下来的，是通过艰苦的劳动获得的，所以，你必须珍惜金钱；同样多的钱在不同人的手中会发挥不同的作用，因而可以说，花钱是一门艺术，所以，你必须学会怎样花钱。通过一个人的用钱方式，可以看出这个人的品德、智慧和性格。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "选苹果、选人生",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      几个学生问哲学家苏格拉底：“人生是什么？”苏格拉底把他们带到一片苹果树林，要求大家从树林的这头走到那头，每人挑选一只自己认为最大最好的苹果。不许走回头路，不许选择两次。在穿过苹果林的过程中，学生们认真细致地挑选自己认为最好的果实。等大家来到苹果林的另一端，苏格拉底已经在那里等候他们了。他笑着问学生：“你们都挑到了自己最满意的果子吗?”大家你看看我，我看看你，都没有回答。苏格拉底见状，又问：“怎么啦，难道你们对自己的选择不满意？”“老师，让我再选择一次吧，”一个学生请求说，“我刚走进果林时，就发现了一个很大很好的苹果．但我还想找一个更大更好的。当我走到果林尽头时，才发现第一次看到的那个就是最大最好的。”另一个学生紧接着说：“我和他恰好相反。我走进果林不久，就摘下了一个我认为最大最好的果子。可是，后来我又发现了更好的。所以，我有点后悔。”“老师．让我们再选择一次吧!”其他学生也不约而同地请求。苏格拉底笑了笑，然后坚定地摇了摇头，语重心长地说：“孩子们，这就是人生──人生就是一次次无法重复的选择。”
      
【温馨提示】

      面对无法回头的人生，我们只能做三件事：郑重地选择，争取不留下遗憾；如果遗憾了，就理智地面对它，然后争取改变；",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "上帝的一分钟",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一个替人割草的男孩打电话给斯宾塞太太：“您需不需要割草工？”
      回答说：“不需要了，我已有了割草工。”
      男孩又说：“我会帮您拔掉花丛中的杂草。”
      斯宾塞太太回答：“我的割草工也做了。”
      男孩又说：“我会帮您把走道两旁的草剪齐。”
      斯宾塞太太说：“我请的割草工正是这样做的。谢谢你，你再到别的地方问问吧。”男孩便挂了电话。
      男孩的朋友问他：“你不是就在斯宾塞太太家修剪草坪吗？为什么还要打这样的电话？”
      男孩说：“我只是想知道我做得有多好！”
      
【温馨提示】

      ：只有不断了解别人对你的评价，才有可能知道自己的长处与短处；只有不断改进工作，你才会永远有工作。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "9.9 秒的启示",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1968年，在墨西哥奥运会的百米赛道上，美国选手吉?海因斯撞线后，激动地看着运动场上的计时牌。当指示器打出9.9秒的字样时，他摊开双手，自言自语地说了一句话。这一情景通过电视转播，至少有好几亿人看到。可是，由于身边没有话筒，海因斯到底说了什么，谁都不知道。
      1984年，洛杉矶奥运会前夕，一位叫戴维?帕尔的记者在办公室回放奥运会的资料片，再次看到海因斯的镜头，这是人类历史上第一次在百米赛道上突破10秒大关。看到自己破记录的那一瞬，海因斯一定说了一句不同凡响的话，但这一新闻点，竟被现场的四百多名记者疏忽了。戴维?帕尔决定采访海因斯，问问他当时到底说了一句什么话。戴维?帕尔很快找到海因斯，问起16年前的事，海因斯竟然毫无印象，甚至否认当时说过什么话。戴维?帕尔说：“你确实说了，有录像带为证。”海因斯看完帕尔带去的录像带，笑了。他说：“难道你没听见吗？我说：‘上帝啊，那扇门原来是虚掩的’。”谜底揭开后，戴维?帕尔对海因斯进行了深入采访。自从欧文斯创造了10.3秒的成绩后，以詹姆斯?格拉森医生为代表的医学界断言，人类的肌肉纤维所承载的运动极限，不会超过每秒10米。海因斯说：“30年来，这一说法在田径场上非常流行，我也以为这是真理。但是，我想，自己至少应该跑出10.1秒的成绩。每天，我以最快的速度跑5公里，我知道百米冠军不是在百米赛道上练出来的。当我在墨西哥奥运会上看到自己9.9秒的记录后，惊呆了。原来，10秒这个门不是紧锁的，而是虚掩的，就像终点那根横着的绳子一样。”后来，戴维?帕尔撰写了一篇报道，填补了墨西哥奥运会留下的一个空白。不过，人们认为它的意义不限于此，海因斯的那句话，给世人留下的启迪更为重要。
      
【温馨提示】

      你付出真诚，会发现友爱之门是虚掩的；你付出智慧，会发现财富之门是虚掩的；你付出艰辛，会发现成功之门是虚掩的。总之，在我们这个多彩的人间，除了牢门是紧锁的，其他的门都是虚掩的。打开心扉，不要自己将自己关在门外。大胆地伸出手，你会推开一个崭新的世界。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "圣?贝纳特学院的铅笔理论",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      美国纽约有一所穷人学校，数十年来，该校的毕业生在纽约警察局的犯罪记录最低。这是为什么？一位研究者通过对该校毕业生的问卷调查，得到了一个奇怪的答案──因为该学校的学生都知道铅笔有多少种用途。
      原来在这所学校，学生入学后接受的第一堂课就是：一支铅笔有多少种用途。在课堂上，孩子们明白了铅笔不仅有写字这种最普通的用途，必要时还能用来做尺子画线；作为礼品送人表示友爱。当作商品出售获得利润，笔芯磨成粉后可做润滑粉，演出时也可临时用于化妆，削下的木屑可以做成装饰画；一支铅笔按相等的比例锯成若干份，可以做成一副象棋，可以当作玩具车的轮子；在野外探险时，铅笔抽掉芯还能被当成吸管喝石缝中的泉水，在遇到坏人时，削尖的铅笔还能当作自卫的武器……通过这一课，老师让学生们懂得了：拥有眼睛、鼻子、耳朵、大脑和手脚的人更是有无数种用途，并且任何一种用途都足以使一个人生存下去。这种教育的结果是，从这所学校毕业的学生，无论他们的处境如何，都生活得非常快乐，因为他们永远对未来充满希望。
      这所学校就是圣?贝纳特学院。对它进行研究的是一位名叫普热罗夫的捷克籍法学博士，他原打算借研究为名拖延在美国的时间，以便找到一份与法学有关的工作。这份奇怪的答案使他放弃了在美国找工作的想法并立即返回国内。目前他已经是捷克最大一家网络公司的总裁。
      
【温馨提示】

      如果你在生活中遭遇了挫折，譬如破产、譬如失业、譬如辍学……你能否想一想铅笔的用途呢？假若一个人知道铅笔有多少种用途，他一定会觉得人生的道路又宽又多。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "成功与贫富无关",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      伊尔?布拉格是美国历史上第一位荣获普利策新闻奖的黑人记者。他勇敢勤奋，功绩卓越，创造了美国新闻史上的一个奇迹。他在回忆自己童年经历时说：“我们家很穷，父母都靠卖苦力为生。那时，我父亲是一名水手，他每年都要往返于大西洋各个港口之间。我一直认为，像我们这样地位卑微的黑人是不可能有什么出息的，也许一生只会像父亲所工作的船只一样，漂泊不定。”
      伊尔?布拉格9岁那年，父亲带他去参观梵高的故居．在那张著名的吱嘎作响的小木床和那双龟裂的皮鞋面前，布拉格好奇地问父亲：“梵高不是世界上最著名的大画家吗？他难道不是百万富翁？”父亲回答他说：“梵高的确是世界著名的画家，同时，他也是一个和我们一样的穷人，而且是一个连妻子都娶不上的穷人。”又过了一年，父亲带着布拉格去了丹麦，在童话大师安徒生墙壁斑驳的故居，布拉格又困惑地问父亲：“安徒生不是生活在皇宫里吗？可是，这里的房子却这样破旧。”父亲答道：“安徒生是个砖匠的儿子，他生前就住在这栋残破的阁楼里．皇宫只在他的童话里才会出现。”从此，布拉格的人生观完全改变。他不再自卑，不再以为只有那些有钱有地位的人才会出人头地。他说：“我庆幸有位好父亲，他让我认识了梵高和安徒生，而这两位伟大的艺术家又告诉我，人能否成功与贫富毫无关系。”
      
【温馨提示】

      现实生活中，我们常常看到这样一些人，他们会以自己的出身来确定自己未来的生活前景；他们经常会因自己角色的卑微，而用可怜的声音与世界对话；他们总是因暂时的生活窘迫而放弃了自己的梦想；他们总是因其貌不扬被人歧视而低下了充满智慧的头颅。不要用卑微的姿态面对世界。一个人只要知道自己要去哪里，全世界都会给他让路。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "找到自己就找到了世界",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1947年，美孚石油公司董事长贝里奇到开普敦巡视工作，在卫生间里，看到一位黑人小伙子正跪在地上擦洗黑污的水渍，并且每擦一下，就虔诚地叩一下头。贝里奇感到很奇怪，问他为何如此？黑人答道：“我在感谢一位圣人。”贝里奇问他为何要感谢那位圣人？小伙子说：“是他帮助我找到了这份工作，让我终于有了饭吃。”贝里奇笑了，说：“我曾经也遇到一位圣人，他使我成了美孚石油公司的董事长，你愿意见他一下吗？”小伙子说：“我是个孤儿，从小靠锡克教会养大，我一直都想报答养育过我的人。这位圣人若能使我吃饱之后，还有余钱，我很愿去拜访他。”贝里奇说：“你一定知道，南非有一座有名的山，叫大温特胡克山。据我所知，那上面住着一位圣人，能为人指点迷津，凡是遇到他的人都会前程似锦。20年前，我到南非登上过那座山，正巧遇上他，并得到他的指。假如你愿意去拜访，我可以向你的经理说情，准你一个月的假。”这位年轻的小伙子是个虔诚的锡克教徒，很相信神的帮助，他谢过贝里奇后就真的上路了。
      30天的时间里，他一路劈荆斩棘，风餐露宿，终于登上了白雪覆盖的大温特胡克山。然而，他在山顶徘徊了一天，除了自己，什么都没有遇到。黑人小伙子很失望地回来丁。他见到贝里奇后说的第一句话是：“董事长先生，一路我处处留意，但直至山顶，我发现，除我之外，根本没有什么圣人。”贝里奇说：“你说得很对，除你之外，根本没有什么圣人。因为，你自己就是圣人。”
      20年后，这位黑人小伙子做了美孚石油公司开普敦分公司的总经理，他的名字叫贾姆讷。在一次世界经济论坛峰会上，他作为美孚石油公司的代表参加了大会。在面对众多记者的提问时，关于自己传奇的一生，他说了这么一句话：“发现自己的那一天，就是人生成功的开始。能创造奇迹的人，只有自己。”
      
【温馨提示】

      人使用最多的一个词是“我”，最视而不见的也是“我”。一个看不见“我”的人既不知道自己能做什么，也不知道自己该做什么。因为看不见自己，就崇拜他人、偶像，而自己就消失在芸芸大众之中。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "冠军的愤怒",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      1965年9月7日，世界台球冠军争夺赛在美国纽约举行。路易斯?弗克斯的得分一路遥遥领先，只要再得几分便可稳拿冠军了。就在最后一场决赛开始不久，他发现一只苍蝇落在主球上，于是挥杆将苍蝇赶走了。可是，当他俯身准备击球的时候，那只苍蝇又飞了回来。在观众的笑声中，他再一次扬起手赶走了苍蝇。他的情绪已经被这只讨厌的小动物破坏了，而且更为糟糕的是，它好像是有意跟他作对，等他一回到球台，苍蝇就又飞落到主球上，引得周围的观众哈哈大笑。路易斯?弗克斯的心境恶劣到了极点，终于失去理智，愤怒地用球杆去击打苍蝇。不幸球杆碰动了主球，裁判判他击球，因此他失去了一轮机会。路易斯?弗克斯方寸大乱，接着连连失利，而他的对手约翰?迪瑞则愈战愈勇，一步步赶上并超过了他，最后夺走了冠军金牌。第二天清早，人们在河里发现了路易斯．弗克斯的尸体，他因无法接受这样的结果而投河自杀了！
      一只小小的苍蝇，竟然击倒了所向无敌的世界冠军！这是一件不该发生的事情。其实，路易斯．弗克斯完全可以采取另一种做法，那就是：击自己的球，不要理睬苍蝇。当主球飞速奔向既定目标的时候，那只苍蝇还站得住吗？
      老虎自恃是森林之王，有一天觅食时遇到了一只飞来飞去的牛虻，老虎生气地喝道：“不要在我眼皮下打扰我，否则我就吃掉你！”“嘻嘻，只要你够得着就来吃呀。”牛虻一面嘲笑老虎，一面飞到老虎鼻子上吸血。老虎用爪子来抓，牛虻又飞到虎背上钻进虎皮中吸血。老虎恼怒地用钢鞭一样的尾巴驱赶牛虻，但牛虻不断地转移位置，不停地狠狠叮咬。老虎躺在地上打滚妄图压死牛虻，牛虻立刻飞走了。但没过一会儿，它又回到老虎的鼻尖上。就这样，一只老虎在和牛虻的搏斗中，活活累死了。老虎其实没有必要去在乎一只牛虻，它的烦恼和灾难不是因为牛虻，而是因为它自己。
      
【温馨提示】

      人生路上，有很多影响我们的“苍蝇”和“牛虻”，但我们得记住自己要做什么，不要在乎这些身外的干扰，才不至于因小而失大。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "瓦伦达心态",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      心理学上有一种“瓦伦达心态”。这个理论，来源于一个真实的故事。
      瓦伦达是美国著名的高空钢索表演者，在一次重大的表演中，不幸失足身亡。他的妻子事后说，我知道这一次一定要出事，因为他上场前总是不停地说：“这次太重要了，不能失败，绝不能失败。”而以往他却不是这样。每次表演这前，他只想着“走钢索”，并专心为此做准备，根本不去管其他的事情，更不会为“成功”或“失败”而担心。
      后来，人们就把专心致志于做某事，而不去管这件事的意义和结果，不患失的心态，叫做“瓦伦达心态”。
      美国斯坦福大学的一项研究也表明，人大脑里的某一图像会像实际情况那样刺激人的神经系统。比如，当一个高尔夫球手击球前一再告诉自己“不要把球打进水里”时，他的大脑里往往就会出现“球掉进水里”的情景。这一情景会指挥他的行到结果事情不是向他希望的那样发展，而是向他害怕的方向发展──这时候，球大多都会掉进水里。
      这项研究从另一个方面证实了瓦伦达心态。
      
【温馨提示】

      当你已经开始做一件事的时候，就不要再考虑与做这件事无关的问题，不要让功利心和由此引出的担忧干扰你的行动。专心去做那件事的时候，就不会再考虑成功或者失败。没有了成败的忧虑，人就自然变得轻松自如。害怕失败就是最大的失败。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "最后一个鸡蛋",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      整个鸡家族都不甘心自己在地球上的地位——任人宰割，供人食用。鸡王决定扭转鸡家族的处境，于是就召集幕僚想办法。
      “要想人类放弃吃鸡很难。”一个幕僚说。他曾经用十斤鸡蛋收买了一位写东西的人，让他在报上写文章说，吃鸡容易患癌症！可人类还是照吃不误。
      “依我看，人类吃咱们并不是因为恨咱们，而是说明他们离不开咱们。”另一位年纪大点的幕僚说，“不如，大王下一道圣旨，命令所有母鸡停止下蛋，保证人类不会再像现在这样对待咱们了。”
      “停止下蛋？”鸡王吃了一惊。
      “对，停止下蛋。没有鸡蛋就不会再有小鸡出生，光这一点，就够人类伤脑筋的了。”幕僚说。
      五分钟后，鸡王向整个鸡家族下达了圣旨：停止下蛋！
      当人类发现全世界所有的母鸡都不下蛋了时，地球上只剩下一个鸡蛋了。
      人类意识到问题的严重性了，地球上的鸡吃一只将少一只，最终灭绝。
      人类为此专门召开了一个紧急会议，会议的结果是制定了一部关于保护珍稀动物“鸡”的法律。鸡终于改变了卑微的地位，加入珍稀动物的行列。
      人们以拥有一只鸡为荣，狗和猫的地位一落千丈。在公园里常见到牵着鸡散步的达官贵人。一只鸡的售价已高达上千美元。
      地球上惟一的那个鸡蛋更是价值连城，光是守护它的军队就有两个旅。这
      鸡蛋的妈妈是最后一个听到鸡王圣旨的，于是，她成了地球上惟一一个鸡蛋的母亲。人们将延续鸡的生命的期望寄托在她身上，人们给她吃最好的食物，为她建造了豪华的住宅，还让她上报纸上电视。
      这只母鸡日益显赫的地位渐渐引起了鸡家族其他成员的不满。同样是鸡，凭什么她吃的好，住的好，还能上电视？就因为她生了地球上最后一个蛋！
      于是，在某一天的上午，另一只母鸡违抗鸡王的圣旨，公然下了一个蛋。这只母鸡立刻成了全球新闻的焦点，她的待遇马上超过了前一只母鸡。
      鸡家族受到了强烈的震动。
      紧接着第三个鸡蛋出生了。
      第四个鸡蛋问世了。
      第五个鸡蛋诞生了。
      第六个鸡蛋降临了。
      ……
      鸡王感到大事不妙，连忙再次降旨制止母鸡下蛋，但他已经控制不了他的臣民了。所有的母鸡都想比别的鸡过得好，她们拼命地下蛋。
      于是，保护鸡的法律取消了。没有人牵鸡去公园了。鸡又重新出现在餐桌上了。
      鸡王不甘心，还想继续降旨制止母鸡下蛋，被幕僚劝阻了。幕僚说：“没有必要。因为人类已经掌握了对付鸡不下蛋的方法。”
      
【温馨提示】

      堡垒都是从内部攻破的。",
            group10));

            group10.Items.Add(new SampleDataItem("Group-4-Item-3",
            "烈马和女孩",
            "Item Subtitle: 3",
            "Assets/title/10.png",
            "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
            @"
      一天早晨，当泰姆波拉计划骑马出行的时候，雇工马赫告诉她这样一个消息：“它太倔了，泰姆波拉夫人。我曾经花了一个小时去费劲地抓那匹撒野的马，可还是没有成功。”正在这个时候，泰姆波拉夫人12岁的小女儿出来了。她名詹妮。小詹妮活泼可爱、长着棕色头发、连眼睛也是棕色的。听到母亲和雇工的谈话，詹妮说：“妈妈你就放心吧，我可以让那匹马听你的话。”但是，那个雇工用了那么长的时间还没有把马驯服，他们甚至声称那匹马根本就无法驯服。泰姆波拉夫人疑惑地看着詹妮，担心她会做什么傻事。詹妮微笑着说：“马赫肯定是不能驯服它的，但是我能。”“我的孩子，你可要小心些，他们说那匹马发起怒来就像一头野兽。不要在它身上花太多的时间，否则就耽误你去学校上课了。”泰姆波拉夫人警告詹妮。“不会的，妈妈，我会让它像一个听话的小狗一样自己到我身边来的。”詹妮一边快乐地说着，一边戴上她的大草帽，然后就独自去放马场了。一匹小马听到詹妮衣裙的沙沙声，便竖起了耳朵，用鼻子嗅了嗅，那
      甜甜的味道正是小女孩儿身上的味道。它挺直了头，准备迎接小姑娘的到来。“派林！噢，派林！”詹妮一边摸着小马的头，一边大声朝远处呼叫着她要找的那匹马的名字。这匹漂亮的马听到她的呼唤，立刻就把头转了过。当它看到小詹妮的时候，快步跑到篱笆边，低下头亲热地蹭着小女孩的肩。
      那位雇工对它那么粗暴，它才不会听那个人的话呢！它能明白，眼前的这个小女孩儿曾经用她那柔软的小手抚摸过它，现在它要用爱来回赠这个小女孩。派林跟着詹妮来到泰姆波拉夫人眼前，它低着头乖乖地等待着夫人下边出发的命令。
      
【温馨提示】

      人对动物的认识，并不一定比动物对人的认识更多。动物知道谁爱它，谁不爱它。而很多人却不知道动物的这种心事。不要以为人类比动物聪明，这种狭隘的观念，让我们变得比一些动物更残忍，比一些动物更愚昧。",
            group10));


            this.AllGroups.Add(group10);
            #endregion
        }
    }
}
