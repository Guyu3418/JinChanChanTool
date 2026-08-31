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
            return GetCurrentLineUp().SubLineUps[变阵索引];
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
            if (index >= 0 && index < GetCurrentLineUp().SubLineUps.Count)
            {
                变阵索引 = index;
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
        /// 获取当前阵容的全部分支。
        /// </summary>
        public IReadOnlyList<SubLineUp> GetSubLineUps()
        {
            return GetCurrentLineUp().SubLineUps;
        }

        /// <summary>
        /// 从当前分支复制创建一个带有名称和玩法说明的新分支。
        /// </summary>
        public bool AddSubLineUp(string name, string description)
        {
            LineUp currentLineUp = GetCurrentLineUp();
            if (currentLineUp.SubLineUps.Count >= LineUp.MaxSubLineUpCount)
            {
                return false;
            }

            string branchName = NormalizeSubLineUpName(name);
            if (string.IsNullOrWhiteSpace(branchName))
            {
                return false;
            }

            // 新分支以当前分支为基础，保留用户已有的英雄、装备和站位安排。
            SubLineUp newSubLineUp = CloneSubLineUp(GetCurrentSubLineUp());
            newSubLineUp.Name = branchName;
            newSubLineUp.Description = NormalizeSubLineUpDescription(description);
            currentLineUp.SubLineUps.Add(newSubLineUp);
            变阵索引 = currentLineUp.SubLineUps.Count - 1;
            NotifyLineUpChanged();
            return true;
        }

        /// <summary>
        /// 删除指定分支。删除唯一分支时以空的未命名分支保持阵容结构有效。
        /// </summary>
        public bool DeleteSubLineUp(int index)
        {
            List<SubLineUp> subLineUps = GetCurrentLineUp().SubLineUps;
            if (index < 0 || index >= subLineUps.Count)
            {
                return false;
            }

            if (subLineUps.Count == 1)
            {
                // 最后一个分支被删除后创建新的空分支，避免当前分支索引失效。
                subLineUps[0] = new SubLineUp
                {
                    Name = LineUp.UnnamedSubLineUpName,
                    Description = string.Empty
                };
                变阵索引 = 0;
                NotifyLineUpChanged();
                return true;
            }

            subLineUps.RemoveAt(index);
            if (变阵索引 > index)
            {
                变阵索引--;
            }
            else if (变阵索引 == index)
            {
                变阵索引 = Math.Min(index, subLineUps.Count - 1);
            }

            NotifyLineUpChanged();
            return true;
        }

        /// <summary>
        /// 更新指定分支的名称和玩法说明。
        /// </summary>
        public bool UpdateSubLineUpMetadata(int index, string name, string description)
        {
            List<SubLineUp> subLineUps = GetCurrentLineUp().SubLineUps;
            if (index < 0 || index >= subLineUps.Count)
            {
                return false;
            }

            string branchName = NormalizeSubLineUpName(name);
            if (string.IsNullOrWhiteSpace(branchName))
            {
                return false;
            }

            SubLineUp targetSubLineUp = subLineUps[index];
            targetSubLineUp.Name = branchName;
            targetSubLineUp.Description = NormalizeSubLineUpDescription(description);
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
            return lineUp.SubLineUps;
        }

        private static void NormalizeLineUp(LineUp lineUp)
        {
            List<SubLineUp> subLineUps = (lineUp.SubLineUps ?? [])
                .Where(subLineUp => subLineUp != null)
                .ToList();
            bool isLegacyFixedStageLayout = subLineUps.Count == 3 &&
                                            subLineUps.All(subLineUp => string.IsNullOrWhiteSpace(subLineUp.Name));

            foreach (FlexBranch legacyFlexBranch in lineUp.LegacyFlexBranches ?? [])
            {
                if (legacyFlexBranch == null)
                {
                    continue;
                }

                SubLineUp migratedSubLineUp = legacyFlexBranch.SubLineUp ?? new SubLineUp();
                migratedSubLineUp.Name = legacyFlexBranch.Name;
                migratedSubLineUp.Description = legacyFlexBranch.Description;
                subLineUps.Add(migratedSubLineUp);
            }
            lineUp.LegacyFlexBranches = [];

            if (subLineUps.Count == 0)
            {
                subLineUps.Add(new SubLineUp());
            }

            lineUp.SubLineUps = subLineUps.Take(LineUp.MaxSubLineUpCount).ToList();
            for (int i = 0; i < lineUp.SubLineUps.Count; i++)
            {
                NormalizeSubLineUp(lineUp.SubLineUps[i], i, isLegacyFixedStageLayout);
            }
        }

        private static void NormalizeSubLineUp(SubLineUp subLineUp, int index, bool isLegacyFixedStageLayout)
        {
            subLineUp.Name = NormalizeSubLineUpName(subLineUp.Name);
            // 将上一版本自动生成的首分支名称迁移为新的默认名称。
            if (index == 0 && subLineUp.Name == "主分支")
            {
                subLineUp.Name = LineUp.DefaultSubLineUpName;
            }
            if (string.IsNullOrWhiteSpace(subLineUp.Name))
            {
                subLineUp.Name = isLegacyFixedStageLayout && index < 3
                    ? new[] { "前期", "中期", "后期" }[index]
                    : index == 0 ? LineUp.DefaultSubLineUpName : $"分支 {index + 1}";
            }
            subLineUp.Description = NormalizeSubLineUpDescription(subLineUp.Description);
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
                Name = source.Name,
                Description = source.Description,
                LineUpUnits = source.LineUpUnits.Select(unit => new LineUpUnit
                {
                    HeroName = unit.HeroName,
                    EquipmentNames = unit.EquipmentNames?.ToArray() ?? ["", "", ""],
                    Position = unit.Position,
                    PositionLayer = unit.PositionLayer
                }).ToList()
            };
        }

        private static string NormalizeSubLineUpName(string name)
        {
            string normalizedName = (name ?? string.Empty).Trim();
            return normalizedName.Length > SubLineUp.MaxNameLength
                ? normalizedName[..SubLineUp.MaxNameLength]
                : normalizedName;
        }

        /// <summary>
        /// 将玩法说明限制在数据模型允许的范围内。
        /// </summary>
        private static string NormalizeSubLineUpDescription(string description)
        {
            string normalizedDescription = (description ?? string.Empty).Trim();
            return normalizedDescription.Length > SubLineUp.MaxDescriptionLength
                ? normalizedDescription[..SubLineUp.MaxDescriptionLength]
                : normalizedDescription;
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

