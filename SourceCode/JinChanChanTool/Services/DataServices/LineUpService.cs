using JinChanChanTool.DataClass;
using JinChanChanTool.Forms;
using JinChanChanTool.Services.DataServices.Interface;
using JinChanChanTool.Services.Localization;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using static JinChanChanTool.DataClass.LineUp;

namespace JinChanChanTool.Services.DataServices
{
    public class LineUpService : ILineUpService
    {
        private const int MaxLineUpHeroCount = 20;

        /// <summary>
        /// 文件路径列表
        /// </summary>
        private string[] _paths;

        /// <summary>
        /// 阵容文件路径索引
        /// </summary>
        private int _pathIndex;

        /// <summary>
        /// LineUp对象列表，存储多个阵容信息。
        /// </summary>
        private List<LineUp> _lineUps;

        /// <summary>
        /// 阵容索引
        /// </summary>
        private int _lineUpIndex;

        /// <summary>
        /// 变阵索引
        /// </summary>
        private int 变阵索引;

        /// <summary>
        /// 当前 Flex 分支下标；-1 表示当前显示主后期阵容。
        /// </summary>
        private int _flexBranchIndex;

        /// <summary>
        /// 英雄数据服务对象
        /// </summary>
        private IHeroDataService _iHeroDataService;

        /// <summary>
        /// 手动设置服务对象
        /// </summary>
        private IManualSettingsService _iManualSettingsService;

        /// <summary>
        /// 本地化服务对象
        /// </summary>
        private ILocalizationService _iLocalizationService;

        /// <summary>
        /// 最大选择数量
        /// </summary>
        ///
        private int _maxOfChoice;

        /// <summary>
        /// 阵容改变事件
        /// </summary>
        public event EventHandler LineUpChanged;

        /// <summary>
        /// 阵容名改变事件
        /// </summary>
        public event EventHandler LineUpNameChanged;

        #region 初始化
        public LineUpService(IHeroDataService iHeroDataService, IManualSettingsService iManualSettingsService, ILocalizationService iLocalizationService, int maxOfChoice, int lineUpIndex)
        {
            _iHeroDataService = iHeroDataService;
            _iManualSettingsService = iManualSettingsService;
            _iLocalizationService = iLocalizationService;
            _maxOfChoice = Math.Clamp(maxOfChoice, 10, MaxLineUpHeroCount);
            InitializePaths();
            _pathIndex = 0;
            变阵索引 = 0;
            _flexBranchIndex = -1;
            _lineUpIndex = lineUpIndex;
            _lineUps = new List<LineUp>();
        }

        /// <summary>
        /// 初始化文件路径列表
        /// </summary>
        private void InitializePaths()
        {
            string parentPath = Path.Combine(Application.StartupPath, "Resources", "HeroDatas");
            // 确保目录存在
            if (!Directory.Exists(parentPath))
            {
                Directory.CreateDirectory(parentPath);
            }
            // 获取所有子目录名称（仅文件夹）
            string[] subDirs = Directory.GetDirectories(parentPath);
            for (int i = 0; i < subDirs.Length; i++)
            {
                subDirs[i] = Path.Combine(parentPath, subDirs[i]);
            }
            _paths = subDirs;
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 从本地文件加载阵容
        /// </summary>
        public void Load()
        {
            LoadFromFile();
        }

        /// <summary>
        /// 将当前阵容数据保存到本地文件。
        /// </summary>
        public bool Save()
        {
            if (_paths.Length > 0 && _pathIndex < _paths.Length)
            {
                string filePath = Path.Combine(_paths[_pathIndex], "LineUps.json");
                try
                {

                    string json = JsonConvert.SerializeObject(_lineUps, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                    NotifyLineUpNameChanged();
                    return true;
                }
                catch
                {
                    MessageBox.Show($"阵容文件\"{Path.GetFileName(filePath)}\"保存失败\n路径：\n{filePath}",
                                  "文件保存失败",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error
                                  );
                    return false;
                }
            }
            else
            {
                MessageBox.Show($"路径不存在，保存失败！",
                                 "路径不存在",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error
                                 );
                return false;
            }
        }

        /// <summary>
        /// 重新加载，需要获取英雄数据服务对象
        /// </summary>
        /// <param name="countOfHeros"></param>
        /// <param name="countOfLineUps"></param>
        public void ReLoad(IHeroDataService heroDataService)
        {
            _iHeroDataService = heroDataService;
            变阵索引 = 0;
            _flexBranchIndex = -1;
            _lineUpIndex = 0;
            _lineUps = new List<LineUp>();
            LoadFromFile();
        }

        /// <summary>
        /// 新增阵容
        /// </summary>
        /// <param name="lineUpName"></param>
        /// <returns></returns>
        public bool AddLineUp(string lineUpName)
        {
            if(IsLineUpNameAvailable(lineUpName))
            {
                _lineUps.Add(new LineUp(lineUpName));
                _lineUpIndex = _lineUps.Count - 1;
                Save();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 删除当前阵容
        /// </summary>
        /// <returns></returns>
        public bool DeleteLineUp()
        {
            if(_lineUps.Count<=1)
            {
                MessageBox.Show($"至少保留一个阵容，无法删除当前阵容。",
                    "删除失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                    );
                return false;
            }
            if(_lineUpIndex>=0 && _lineUpIndex<_lineUps.Count)
            {                
                _lineUps.RemoveAt(_lineUpIndex);
                // 调整当前阵容索引
                if (_lineUpIndex >= _lineUps.Count)
                {
                    _lineUpIndex = _lineUps.Count - 1;
                }
                Save();                
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断阵容名是否可用（不与现有阵容重名）
        /// </summary>
        /// <param name="name">待检查的阵容名</param>
        /// <returns>可用返回true，已存在返回false</returns>
        public bool IsLineUpNameAvailable(string name)
        {
            return !_lineUps.Any(lineUp => lineUp.LineUpName == name);
        }

        /// <summary>
        /// 检查当前子阵容是否包含指定英雄名称，若包含则将其从子阵容删除，否则将其添加到子阵容。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool AddAndDeleteHero(string name, string[] equipments)
        {
            if (GetCurrentSubLineUp().Contains(name))
            {
                GetCurrentSubLineUp().Remove(name);
                NotifyLineUpChanged();
                return true;
            }
            else
            {
                if (GetCurrentSubLineUp().LineUpUnits.Count >= _maxOfChoice)
                {
                    return false;
                }
                GetCurrentSubLineUp().Add(name, equipments);
                OrderCurrentSubLineUp();
                NotifyLineUpChanged();
                return true;
            }
        }

        /// <summary>
        /// 增加指定英雄名称到当前子阵容(指定装备)，若已存在则不再增加
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool AddHero(string name, string[] equipments)
        {
            if (GetCurrentSubLineUp().Contains(name))
            {
                return false;
            }
            else
            {
                if (GetCurrentSubLineUp().LineUpUnits.Count >= _maxOfChoice)
                {
                    return false;
                }
                GetCurrentSubLineUp().Add(name, equipments);
                OrderCurrentSubLineUp();
                NotifyLineUpChanged();
                return true;
            }
        }

        /// <summary>
        /// 增加指定英雄名称到当前子阵容(不指定装备)，若已存在则不再增加
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool AddHero(string name)
        {
            if (GetCurrentSubLineUp().Contains(name))
            {
                return false;
            }
            else
            {
                if (GetCurrentSubLineUp().LineUpUnits.Count >= _maxOfChoice)
                {
                    return false;
                }
                GetCurrentSubLineUp().Add(name);
                OrderCurrentSubLineUp();
                NotifyLineUpChanged();
                return true;
            }
        }

        /// <summary>
        /// 从当前子阵容删除指定英雄名称，若不存在则不会删除
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool DeleteHero(string name)
        {
            if (GetCurrentSubLineUp().Contains(name))
            {
                GetCurrentSubLineUp().Remove(name);
                NotifyLineUpChanged();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 清空当前子阵容
        /// </summary>
        public void ClearCurrentSubLineUp()
        {
            GetCurrentSubLineUp().LineUpUnits.Clear();
            NotifyLineUpChanged();
        }

        /// <summary>
        /// 替换当前子阵容的英雄列表（用于从推荐阵容导入）
        /// </summary>
        /// <param name="lineUpUnits">要导入的英雄单位列表</param>
        /// <returns>是否替换成功</returns>
        public bool ReplaceCurrentSubLineUp(List<LineUpUnit> lineUpUnits)
        {
            if (lineUpUnits == null)
            {
                return false;
            }

            SubLineUp currentSubLineUp = GetCurrentSubLineUp();
            currentSubLineUp.LineUpUnits.Clear();

            // 添加新的英雄单位，最多保留 20 名。
            for (int i = 0; i < Math.Min(lineUpUnits.Count, _maxOfChoice); i++)
            {
                var unit = lineUpUnits[i];
                // 验证英雄是否存在
                if (_iHeroDataService.GetHeroFromName(unit.HeroName) != null)
                {
                    // 创建新的LineUpUnit，避免引用问题
                    var newUnit = new LineUpUnit
                    {
                        HeroName = unit.HeroName,
                        EquipmentNames = unit.EquipmentNames?.ToArray() ?? ["", "", ""],
                        Position = unit.Position,
                        PositionLayer = unit.PositionLayer
                    };
                    currentSubLineUp.LineUpUnits.Add(newUnit);
                }
            }

            // 按Cost排序
            OrderCurrentSubLineUp();
            NotifyLineUpChanged();
            return true;
        }

        /// <summary>
        /// 修改当前子阵容中指定英雄的装备
        /// </summary>
        /// <param name="heroName">英雄名称</param>
        /// <param name="equipmentIndex">装备槽位索引(0-2)</param>
        /// <param name="equipmentName">新装备名称</param>
        /// <returns>是否修改成功</returns>
        public bool SetHeroEquipment(string heroName, int equipmentIndex, string equipmentName)
        {
            bool result = GetCurrentSubLineUp().SetEquipment(heroName, equipmentIndex, equipmentName);
            if (result)
            {
                NotifyLineUpChanged();
            }
            return result;
        }

        /// <summary>
        /// 获取阵容集合
        /// </summary>
        /// <returns></returns>
        public List<LineUp> GetLineUps()
        {
            return _lineUps;
        }

        /// <summary>
        /// 获取当前阵容对象
        /// </summary>
        /// <returns></returns>
        public LineUp GetCurrentLineUp()
        {
            return _lineUps[_lineUpIndex];
        }

        /// <summary>
        /// 获取当前变阵
        /// </summary>
        /// <returns></returns>
        public SubLineUp GetCurrentSubLineUp()
        {
            if (_flexBranchIndex >= 0)
            {
                FlexBranch flexBranch = GetCurrentFlexBranch();
                if (flexBranch != null)
                {
                    return flexBranch.SubLineUp;
                }
            }
            return _lineUps[_lineUpIndex].SubLineUps[变阵索引];
        }

        /// <summary>
        /// 设置阵容下标
        /// </summary>
        /// <param name="lineUpIndex"></param>
        public bool SetLineUpIndex(int lineUpIndex)
        {
            if (lineUpIndex >= 0 && lineUpIndex < _lineUps.Count)
            {
                _lineUpIndex = lineUpIndex;
                变阵索引 = 0;
                _flexBranchIndex = -1;

                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取阵容下标
        /// </summary>
        /// <returns></returns>
        public int GetLineUpIndex()
        {
            return _lineUpIndex;
        }

        /// <summary>
        /// 设置当前变阵下标
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool SetSubLineUpIndex(int index)
        {
            if (index >= 0 && index < 3)
            {
                变阵索引 = index;
                _flexBranchIndex = -1;
                NotifyLineUpChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取当前变阵下标
        /// </summary>
        /// <returns></returns>
        public int GetSubLineUpIndex()
        {
            return 变阵索引;
        }

        /// <summary>
        /// 获取当前阵容的 Flex 分支。
        /// </summary>
        public IReadOnlyList<FlexBranch> GetFlexBranches()
        {
            return GetCurrentLineUp().FlexBranches;
        }

        /// <summary>
        /// 获取当前选中的 Flex 分支。
        /// </summary>
        public FlexBranch GetCurrentFlexBranch()
        {
            List<FlexBranch> flexBranches = GetCurrentLineUp().FlexBranches;
            if (_flexBranchIndex < 0 || _flexBranchIndex >= flexBranches.Count)
            {
                return null;
            }
            return flexBranches[_flexBranchIndex];
        }

        /// <summary>
        /// 切换当前 Flex 分支。-1 代表主后期阵容。
        /// </summary>
        public bool SetFlexBranchIndex(int index)
        {
            List<FlexBranch> flexBranches = GetCurrentLineUp().FlexBranches;
            if (index < -1 || index >= flexBranches.Count)
            {
                return false;
            }

            变阵索引 = 2;
            _flexBranchIndex = index;
            NotifyLineUpChanged();
            return true;
        }

        /// <summary>
        /// 获取当前 Flex 分支下标。
        /// </summary>
        public int GetFlexBranchIndex()
        {
            return _flexBranchIndex;
        }

        /// <summary>
        /// 从当前后期阵容复制出一个新的 Flex 分支。
        /// </summary>
        public bool AddFlexBranch(string name)
        {
            LineUp currentLineUp = GetCurrentLineUp();
            if (currentLineUp.FlexBranches.Count >= FlexBranch.MaxBranchCount)
            {
                return false;
            }

            string branchName = NormalizeFlexBranchName(name);
            if (string.IsNullOrWhiteSpace(branchName))
            {
                branchName = $"Flex 分支 {currentLineUp.FlexBranches.Count + 1}";
            }

            currentLineUp.FlexBranches.Add(new FlexBranch
            {
                Name = branchName,
                SubLineUp = CloneSubLineUp(currentLineUp.SubLineUps[2])
            });
            _flexBranchIndex = currentLineUp.FlexBranches.Count - 1;
            变阵索引 = 2;
            NotifyLineUpChanged();
            return true;
        }

        /// <summary>
        /// 删除指定 Flex 分支。
        /// </summary>
        public bool DeleteFlexBranch(int index)
        {
            List<FlexBranch> flexBranches = GetCurrentLineUp().FlexBranches;
            if (index < 0 || index >= flexBranches.Count)
            {
                return false;
            }

            flexBranches.RemoveAt(index);
            _flexBranchIndex = -1;
            变阵索引 = 2;
            NotifyLineUpChanged();
            return true;
        }

        /// <summary>
        /// 更新当前 Flex 分支的元数据。
        /// </summary>
        public bool UpdateCurrentFlexBranch(string name, string description)
        {
            FlexBranch flexBranch = GetCurrentFlexBranch();
            string branchName = NormalizeFlexBranchName(name);
            if (flexBranch == null || string.IsNullOrWhiteSpace(branchName))
            {
                return false;
            }

            flexBranch.Name = branchName;
            flexBranch.Description = (description ?? string.Empty).Trim();
            if (flexBranch.Description.Length > FlexBranch.MaxDescriptionLength)
            {
                flexBranch.Description = flexBranch.Description[..FlexBranch.MaxDescriptionLength];
            }
            return true;
        }

        /// <summary>
        /// 设置指定下标阵容名称
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool SetLineUpName(string name)
        {
            // 检查是否使用了保留名称"添加阵容"
            if (name == "添加阵容")
            {
                MessageBox.Show($"阵容名称\"{name}\"为保留名称，请使用其他名称。",
                    "保留名称",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                    );
                return false;
            }

            if (GetCurrentLineUp().LineUpName != name && !IsLineUpNameAvailable(name))
            {
                MessageBox.Show($"阵容名称\"{name}\"已存在，请使用其他名称。",
                    "名称已存在",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                    );
                return false;
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                GetCurrentLineUp().LineUpName = name;
                return true;
            }
            return false;
        }
    
        /// <summary>
        /// 获取最大选择数量
        /// </summary>
        /// <returns></returns>
        public int GetMaxSelect()
        {
            return _maxOfChoice;
        }

        /// <summary>
        /// 获取当前阵容数量
        /// </summary>
        /// <returns></returns>
        public int GetMaxLineUpCount()
        {
            return _lineUps.Count;
        }

        /// <summary>
        /// 设置文件路径索引
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool SetFilePathsIndex(string season)
        {
            int selectedIndex = 0;
            bool isFound = false;
            if (!string.IsNullOrEmpty(season))
            {
                for (int i = 0; i < _paths.Length; i++)
                {
                    if (Path.GetFileName(_paths[i]).Equals(season, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedIndex = i;
                        isFound = true;
                        break;
                    }
                }
            }
            if (_paths.Length > 0)
            {
                _pathIndex = Math.Min(selectedIndex, _paths.Length - 1);
            }
            return isFound;
        }
        #endregion

        /// <summary>
        /// 从本地文件加载阵容数据，若失败则创建默认阵容文件并加载。
        /// </summary>
        /// <param name="countOfHeros"></param>
        /// <param name="TotalLineups"></param>
        /// <returns></returns>
        private void LoadFromFile()
        {
            _lineUps.Clear();
            if (_paths.Length > 0 && _pathIndex < _paths.Length)
            {
                string filePath = Path.Combine(_paths[_pathIndex], "LineUps.json");
                try
                {

                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show($"找不到阵容文件\"{Path.GetFileName(filePath)}\"\n路径：\n{filePath}\n将创建新的阵容文件。",
                                    "文件缺失",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning
                                    );
                        LoadDefaultLineups();
                        Save();
                        return;
                    }

                    string json = File.ReadAllText(filePath);
                    if (string.IsNullOrEmpty(json))
                    {
                        MessageBox.Show($"阵容文件\"{Path.GetFileName(filePath)}\"内容为空。\n路径：\n{filePath}\n将创建新的阵容文件。",
                                   "文件为空",
                                   MessageBoxButtons.OK,
                                   MessageBoxIcon.Warning
                                   );
                        LoadDefaultLineups();
                        Save();
                        return;
                    }


                    List<LineUp> temp = JsonConvert.DeserializeObject<List<LineUp>>(json);
                    if (temp.Count == 0)
                    {
                        int i = 1;
                        while (!IsLineUpNameAvailable($"阵容{i}"))
                        {
                            i++;
                        }
                        temp.Add(new LineUp($"阵容{i}"));
                    }
                    foreach (LineUp lineUp in temp)
                    {
                        NormalizeLineUp(lineUp);
                    }
                    _lineUps.AddRange(temp);
                    RemoveDuplicateLineUps();
                   

                    //检查阵容数据是否与英雄数据冲突
                    foreach (LineUp lineUp in _lineUps)
                    {
                        foreach (SubLineUp subLineUp in GetAllSubLineUps(lineUp))
                        {
                            foreach (LineUpUnit sUnit in subLineUp.LineUpUnits)
                            {
                                if (_iHeroDataService.GetHeroFromName(sUnit.HeroName) == null)
                                {
                                    MessageBox.Show($"阵容文件“{Path.GetFileName(filePath)}”与英雄配置文件“HeroData.json”不匹配，将创建新的阵容文件。",
                                             "阵容文件与英雄配置文件冲突",
                                             MessageBoxButtons.OK,
                                             MessageBoxIcon.Warning);
                                    LoadDefaultLineups();
                                    Save();
                                    return;
                                }
                            }
                        }


                    }

                    //将阵容按照Cost排序
                    foreach (LineUp lineUp in _lineUps)
                    {
                        foreach (SubLineUp subLineUp in GetAllSubLineUps(lineUp))
                        {
                            SortSubLineUp(subLineUp);
                        }
                    }

                    SaveWithoutNotify();
                }
                catch
                {
                    MessageBox.Show($"阵容文件“{Path.GetFileName(filePath)}”格式错误\n路径：\n{filePath}\n将创建新的阵容文件。",
                                  "文件格式错误",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Warning
                                  );
                    LoadDefaultLineups();
                    Save();
                }

            }
            else
            {
                MessageBox.Show($"阵容配置文件夹不存在",
                    "文件夹不存在",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                    );
            }
        }

        private void LoadDefaultLineups()
        {
            _lineUps.Clear();
            _lineUps.Add(new LineUp($"阵容1"));           
        }

        /// <summary>
        /// 移除列表中的同名阵容，只保留下标最小的那一个
        /// </summary>
        /// <param name="lineUps">待检查的阵容列表</param>
        private void RemoveDuplicateLineUps()
        {
            HashSet<string> seenNames = new HashSet<string>();
            int i = 0;
            while (i < _lineUps.Count)
            {
                string lineUpName = _lineUps[i].LineUpName;
                if (seenNames.Contains(lineUpName))
                {
                    // 名称已存在，移除当前项（它是重复的）
                    _lineUps.RemoveAt(i);
                    // 不增加i，因为后面的元素前移了
                }
                else
                {
                    seenNames.Add(lineUpName);
                    i++;
                }
            }
        }

        

        /// <summary>
        /// 仅保存文件，不触发事件（供内部使用，避免循环调用）
        /// </summary>
        /// <returns>是否保存成功</returns>
        private bool SaveWithoutNotify()
        {
            if (_paths.Length > 0 && _pathIndex < _paths.Length)
            {
                string filePath = Path.Combine(_paths[_pathIndex], "LineUps.json");
                try
                {

                    string json = JsonConvert.SerializeObject(_lineUps, Formatting.Indented);
                    File.WriteAllText(filePath, json);                    
                    return true;
                }
                catch
                {
                    MessageBox.Show($"阵容文件\"{Path.GetFileName(filePath)}\"保存失败\n路径：\n{filePath}",
                                  "文件保存失败",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error
                                  );
                    return false;
                }
            }
            else
            {
                MessageBox.Show($"路径不存在，保存失败！",
                                 "路径不存在",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error
                                 );
                return false;
            }
        }

        /// <summary>
        /// 按Cost升序排序当前子阵容的英雄
        /// </summary>
        private void OrderCurrentSubLineUp()
        {
            SortSubLineUp(GetCurrentSubLineUp());
        }

        private void SortSubLineUp(SubLineUp subLineUp)
        {
            List<LineUpUnit> newList = subLineUp.LineUpUnits
                .OrderBy(unit => _iHeroDataService.GetHeroFromName(unit.HeroName).Cost)
                .ToList();
            subLineUp.LineUpUnits.Clear();
            subLineUp.LineUpUnits.AddRange(newList);
        }

        private static IEnumerable<SubLineUp> GetAllSubLineUps(LineUp lineUp)
        {
            foreach (SubLineUp subLineUp in lineUp.SubLineUps)
            {
                yield return subLineUp;
            }

            foreach (FlexBranch flexBranch in lineUp.FlexBranches)
            {
                yield return flexBranch.SubLineUp;
            }
        }

        private static void NormalizeLineUp(LineUp lineUp)
        {
            SubLineUp[] sourceSubLineUps = lineUp.SubLineUps ?? [];
            lineUp.SubLineUps = new SubLineUp[3];
            for (int i = 0; i < lineUp.SubLineUps.Length; i++)
            {
                lineUp.SubLineUps[i] = i < sourceSubLineUps.Length && sourceSubLineUps[i] != null
                    ? sourceSubLineUps[i]
                    : new SubLineUp();
                NormalizeSubLineUp(lineUp.SubLineUps[i]);
            }

            lineUp.FlexBranches ??= [];
            lineUp.FlexBranches = lineUp.FlexBranches.Take(FlexBranch.MaxBranchCount).ToList();
            for (int i = 0; i < lineUp.FlexBranches.Count; i++)
            {
                FlexBranch flexBranch = lineUp.FlexBranches[i] ?? new FlexBranch();
                flexBranch.Name = NormalizeFlexBranchName(flexBranch.Name);
                if (string.IsNullOrWhiteSpace(flexBranch.Name))
                {
                    flexBranch.Name = $"Flex 分支 {i + 1}";
                }
                flexBranch.Description = (flexBranch.Description ?? string.Empty).Trim();
                if (flexBranch.Description.Length > FlexBranch.MaxDescriptionLength)
                {
                    flexBranch.Description = flexBranch.Description[..FlexBranch.MaxDescriptionLength];
                }
                flexBranch.SubLineUp ??= new SubLineUp();
                NormalizeSubLineUp(flexBranch.SubLineUp);
                lineUp.FlexBranches[i] = flexBranch;
            }
        }

        private static void NormalizeSubLineUp(SubLineUp subLineUp)
        {
            subLineUp.LineUpUnits ??= [];
            if (subLineUp.LineUpUnits.Count > MaxLineUpHeroCount)
            {
                subLineUp.LineUpUnits = subLineUp.LineUpUnits.Take(MaxLineUpHeroCount).ToList();
            }
            foreach (LineUpUnit unit in subLineUp.LineUpUnits)
            {
                unit.EquipmentNames ??= ["", "", ""];
                if (unit.EquipmentNames.Length != 3)
                {
                    unit.EquipmentNames = unit.EquipmentNames.Concat(Enumerable.Repeat(string.Empty, 3))
                        .Take(3)
                        .ToArray();
                }
            }
        }

        private static SubLineUp CloneSubLineUp(SubLineUp source)
        {
            return new SubLineUp
            {
                LineUpUnits = source.LineUpUnits.Select(unit => new LineUpUnit
                {
                    HeroName = unit.HeroName,
                    EquipmentNames = unit.EquipmentNames?.ToArray() ?? ["", "", ""],
                    Position = unit.Position,
                    PositionLayer = unit.PositionLayer
                }).ToList()
            };
        }

        private static string NormalizeFlexBranchName(string name)
        {
            string normalizedName = (name ?? string.Empty).Trim();
            return normalizedName.Length > FlexBranch.MaxNameLength
                ? normalizedName[..FlexBranch.MaxNameLength]
                : normalizedName;
        }

        private void NotifyLineUpChanged()
        {
            LineUpChanged?.Invoke(this, EventArgs.Empty);
        }

        private void NotifyLineUpNameChanged()
        {
            LineUpNameChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

