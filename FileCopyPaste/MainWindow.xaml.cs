using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Configuration;
using System.IO;
using Microsoft.Win32;
using FileCopyPaste.classes;
using System.Collections.Specialized;

namespace FileCopyPaste
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] paths = new string[3];
        TreeViewItem[] selectedItem = new TreeViewItem[3];
        FileInfo selectedFile;

        String[] pasteTreeKey = new string[3];
        String refreshTree0Key = "";
        String delKey = "";
        String enterKey = "";
        String newFolderKey = "";
        String copyPathKey = "";
        String changeThemeKey = "";
        String closeWindowKey = "";
        String ignoreFiles = "";
        String ignoreDirs = "";

        List<DataOp.OpenFileApp> openFileAppKeys;
        int currentTheme = 0;

        public MainWindow()
        {
            InitializeComponent();
            readPath();
            readShortcut();
            setBtnText();
            resetTree(0);
            resetTree(1);
            resetTree(2);
        }
        
        private void readShortcut()
        {
            var pasteKey = readKey("key_paste");
            if (!String.IsNullOrEmpty(pasteKey))
            {
                for (int i = 0; i < pasteKey.Split(',').Count() && i < pasteTreeKey.Count(); i++)
                {
                    pasteTreeKey[i] = pasteKey.Split(',')[i];
                }
            }
            refreshTree0Key = readKey("key_refresh_tree0");
            delKey = readKey("key_delete");
            enterKey = readKey("key_enter");
            newFolderKey = readKey("key_new_folder");
            copyPathKey = readKey("key_copy_path");
            ignoreFiles = readKey("ignore_files");
            ignoreDirs = readKey("ignore_dirs");
            openFileAppKeys = DataOp.ReadOpenFileApps();
            changeThemeKey = readKey("key_change_theme");
            closeWindowKey = readKey("key_close_window");
        }

        private void readPath()
        {
            for (var i = 0; i < paths.Length; i++)
            {
                paths[i] = readKey(String.Format("path{0}", i));
                if (String.IsNullOrEmpty(paths[i]) || !Directory.Exists(paths[i]))
                {
                    paths[i] = System.AppDomain.CurrentDomain.BaseDirectory;
                }
            }

        }

        private void setBtnText()
        {
            Btn1.Content = paths[0];
            Btn2.Content = paths[1];
        }

        private int GetNum(object obj)
        {
            int num = -1;
            if (obj != null)
            {
                if (obj == Btn1 || obj == Tree0)
                    num = 0;
                else if (obj == Btn2 || obj == Tree1)
                    num = 1;
                else if (obj == Tree2)
                    num = 2;
            }
            return num;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var num = GetNum(sender);

            var dialog = new FolderSelectDialog();
            dialog.InitialDirectory = paths[num];
            if (dialog.ShowDialog())
            {
                if (!paths[num].Equals(dialog.FileName))
                {
                    paths[num] = dialog.FileName;
                    writeKey(String.Format("path{0}", num), paths[num]);

                    ((Button)(sender)).Content = paths[num];
                    resetTree(num);
                }
            }
        }

        private void resetTree(int num)
        {
            var tree = Tree0;
            bool setTotalFileDisplay = false;
            if (num == 1) tree = Tree1;
            if (num == 2)
            {
                tree = Tree2;
                setTotalFileDisplay = true;
            }
            var items = getDirs(paths[num], 7, setTotalFileDisplay);
            tree.Items.Clear();

            foreach (var item in items)
            {
                tree.Items.Add(item);
            }
        }

        private void refreshTree(TreeView tree)
        {
            var num = GetNum(tree);
            resetTree(num);
            expandTree(tree.Items, selectedItem[num]);
        }

        private void expandTree(ItemCollection items, TreeViewItem target)
        {
            if (items != null && target != null && target.Tag != null)
            {
                var id = (target.Tag as DirectoryInfo).FullName;
                foreach (var itemsingle in items)
                {
                    var it = itemsingle as TreeViewItem;
                    var dirinfo = it.Tag as DirectoryInfo;
                    if (dirinfo.FullName.Equals(id))
                    {
                        //it.IsExpanded = true;
                        it.IsSelected = true;

                        var parent = it.Parent;
                        while (true)
                        {
                            if (parent != null && parent is TreeViewItem)
                            {
                                (parent as TreeViewItem).IsExpanded = true;
                                parent = (parent as TreeViewItem).Parent;
                            }
                            else break;
                        }


                    }
                    else
                    {
                        expandTree(it.Items, target);
                    }
                }
            }
        }
        private List<TreeViewItem> getDirs(string totaldir, int loopNum, bool setTotalDir = false)
        {
            List<TreeViewItem> list = new List<TreeViewItem>();
            var dirinfo = new DirectoryInfo(totaldir);
            if (loopNum > 0)
            { // 最多N层，太多没有必要
                try
                {
                    var dirs = dirinfo.GetDirectories();
                    if (setTotalDir)
                    {
                        dirs = new DirectoryInfo[1] { dirinfo };
                    }

                    foreach (var dir in dirs)
                    {
                        if (!String.IsNullOrEmpty(ignoreDirs))
                        {
                            var ignore = false;
                            foreach (var ignoredir in ignoreDirs.Split(','))
                            {
                                if (ignoredir.Equals(dir.Name))
                                {
                                    ignore = true;
                                    break;
                                }
                            }
                            if (ignore) continue;
                        }
                        TreeViewItem item = new TreeViewItem();
                        item.Header = dir.Name;
                        item.Tag = dir;
                        list.Add(item);

                        var list2 = getDirs(dir.FullName, loopNum - 1);
                        if (list2.Count() > 0)
                        {
                            foreach (var subitem in list2)
                                item.Items.Add(subitem);

                        }
                    }
                }
                catch (Exception e)
                {
                }
            }
            return list;
        }

        private String readKey(String key)
        {
            return DataOp.ReadKey(key);
        }


        private bool writeKey(String key, String value)
        {
            return DataOp.SetKey(key, value);
        }

        private String getItemPath(TreeViewItem item)
        {
            var tag = item.Tag as DirectoryInfo;
            return tag.FullName;
        }

        private bool isIgnoreFile(FileInfo info)
        {
            bool ret = false;
            if (!String.IsNullOrEmpty(ignoreFiles))
            {
                foreach (var name in ignoreFiles.Split(','))
                {
                    if (name.Contains("."))
                    {
                        if (name.Equals(info.Name))
                        {
                            ret = true;
                            break;
                        }
                    }
                    else if (info.Extension.ToLower().Equals("." + name.ToLower()))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;
            var item = tree.SelectedItem as TreeViewItem;
            if (item == null) return;

            var num = GetNum(sender);

            var path = getItemPath(item);

            selectedItem[num] = item;

            if (num > -1)
            {
                ListFile.Items.Clear();
                selectedFile = null;
                var dirinfo = new DirectoryInfo(path);
                foreach (var fileinfo in dirinfo.GetFiles())
                {
                    if (isIgnoreFile(fileinfo))
                    {
                        continue;
                    }
                    var listitem = new ListViewItem();
                    listitem.Content = fileinfo.Name;
                    listitem.Tag = fileinfo;
                    ListFile.Items.Add(listitem);
                }
            }
        }

        private void ListFile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ListView).SelectedItem as ListViewItem;
            if (item != null)
            {
                var fileinfo = item.Tag as FileInfo;
                selectedFile = fileinfo;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Console.WriteLine(e.Key);
            if (isKeyDown(e, pasteTreeKey))
            {
                var num = -1;
                for (int i = 0; i < pasteTreeKey.Count(); i++)
                {
                    if (isKeyDown(e, pasteTreeKey[i]))
                    {
                        num = i;
                        break;
                    }
                }
                var message = "";
                if (selectedFile != null && selectedItem[num] != null)
                {
                    String pasteName = System.IO.Path.Combine(getItemPath(selectedItem[num]), selectedFile.Name);
                    if (!selectedFile.FullName.Equals(pasteName))
                    {
                        if (File.Exists(pasteName))
                        {
                            File.Delete(pasteName);
                        }
                        File.Copy(selectedFile.FullName, pasteName);
                        message = selectedFile.FullName + Environment.NewLine + "copy to" + Environment.NewLine + pasteName;
                    }
                    else
                    {
                        message = "no need to copy";
                    }
                }
                else
                {
                    message = "no file or directory selected";
                }

                MessageBox.Show(message);
            }
            else if (isKeyDown(e, closeWindowKey))
            {
                Close();
            }
            else if (isKeyDown(e, refreshTree0Key))
            {
                refreshTree(Tree0);
            }
            else if (isKeyDown(e, delKey))
            {
                var obj = getFocusedControl();
                if (obj == ListFile && ListFile.SelectedItems != null)
                {
                    var items = ListFile.SelectedItems;
                    var count = items.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var item = items[0];
                        ListFile.Items.Remove(item);
                        var info = ((item as ListViewItem).Tag as FileInfo);
                        info.Delete();
                    }
                }
                else if (obj is TreeView)
                {
                    TreeView tree = obj as TreeView;
                    if (tree.SelectedItem != null)
                    {
                        var info = (tree.SelectedItem as TreeViewItem).Tag as DirectoryInfo;
                        if (MessageBox.Show("delete " + info.Name + " ?") == MessageBoxResult.OK)
                        {
                            try
                            {
                                MyUtils.delDirecotry(info);
                                refreshTree(tree);
                            }
                            catch (Exception e2)
                            {
                                MessageBox.Show("删除失败，最多只能删除2层文件");
                            }
                        }
                    }
                }
            }
            else if (isKeyDown(e, enterKey))
            {
                if (isListViewItemFocused(ListFile.Items) && selectedFile != null)
                {
                    foreach (var openfile in openFileAppKeys)
                    {
                        if (openfile.isSameExt(selectedFile.Extension))
                        {
                            openfile.openFile(selectedFile.FullName);
                            break;
                        }
                    }
                }
            }
            else if (isKeyDown(e, copyPathKey))
            {
                var obj = getFocusedControl();
                if (obj is TreeView && (obj as TreeView).SelectedItem != null)
                {
                    Clipboard.SetText((((((obj as TreeView).SelectedItem) as TreeViewItem).Tag) as DirectoryInfo).FullName);
                }
                else if (obj is ListView && selectedFile != null)
                {
                    Clipboard.SetText(selectedFile.FullName);
                    if (ListFile.SelectedItems.Count > 0) {
                        StringCollection files = new StringCollection();
                        foreach (var item in ListFile.SelectedItems)
                        {
                            files.Add((((item as ListViewItem).Tag as FileInfo).FullName));
                        }
                        Clipboard.SetFileDropList(files);
                    }
                }
            }
            else if (isKeyDown(e, changeThemeKey))
            {
                changeTheme();
            }
            else if (isKeyDown(e, newFolderKey))
            {
                var obj = getFocusedControl();
                if (obj is TreeView)
                {
                    var tree = obj as TreeView;

                    if (tree.SelectedItem != null)
                    {
                        InputBoxItem[] items = new InputBoxItem[] {
                            new InputBoxItem("name", "")
                        };

                        InputBox input = InputBox.Show("CreateDirectory", items, InputBoxButtons.OKCancel);
                        if (input.Result == InputBoxResult.OK)
                        {
                            string name = input.Items["name"];
                            if (!String.IsNullOrEmpty(name))
                            {
                                var info = ((tree.SelectedItem as TreeViewItem).Tag as DirectoryInfo).FullName;
                                Directory.CreateDirectory(System.IO.Path.Combine(info, name));
                                refreshTree(tree);
                            }
                        }
                    }
                }
            }
        }

        private Object getFocusedControl()
        {
            Object obj = null;
            if (isTreeViewItemFocused(Tree0.Items)) obj = Tree0;
            else if (isTreeViewItemFocused(Tree1.Items)) obj = Tree1;
            else if (isTreeViewItemFocused(Tree2.Items)) obj = Tree2;
            else if (isListViewItemFocused(ListFile.Items)) obj = ListFile;
            return obj;
        }
        private bool isTreeViewItemFocused(ItemCollection items)
        {
            bool focused = false;
            if (items != null)
                foreach (var item in items)
                {
                    var it = item as TreeViewItem;
                    focused = isTreeViewItemFocused(it.Items);
                    if (it.IsFocused || focused)
                    {
                        focused = true;
                        break;
                    }
                }
            return focused;
        }
        private bool isListViewItemFocused(ItemCollection items)
        {
            bool focused = false;
            if (items != null)
            {
                foreach (var it in items)
                {
                    if ((it as ListViewItem).IsFocused)
                    {
                        focused = true;
                        break;
                    }
                }
            }
            return focused;
        }
        private bool isKeyDown(KeyEventArgs e, string[] keys)
        {
            bool ret = false;
            if (keys != null)
                foreach (var key in keys)
                {
                    if (isKeyDown(e, key))
                    {
                        ret = true;
                        break;
                    }
                }
            return ret;
        }

        private bool isKeyDown(KeyEventArgs e, string key)
        {
            if (!String.IsNullOrEmpty(key))
            {
                string[] splitkeys = key.ToLower().Split('+');
                bool ctrlkey = false;
                bool shiftkey = false;
                bool altkey = false;
                string actualkey = "";
                for (int i = 0; i < splitkeys.Length; i++)
                {
                    var splitkey = splitkeys[i].Trim();
                    if (splitkey.Equals("ctrl") || splitkey.Equals("ctry"))
                    {
                        ctrlkey = true;
                    }
                    else if (splitkey.Equals("shift"))
                    {
                        shiftkey = true;
                    }
                    else if (splitkey.Equals("alt"))
                    {
                        altkey = true;
                    }
                    else
                    {
                        actualkey = splitkey;
                    }
                }

                if (ctrlkey == ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) &&
                    altkey == ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) &&
                    shiftkey == ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) &&
                    actualkey.ToLower().Equals(e.Key.ToString().ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        private void Tree_GotFocus(object sender, RoutedEventArgs e)
        {
            var tree = sender as TreeView;

            if (tree.SelectedItem != null)
            {
                Tree_SelectedItemChanged(sender, null);
            }
        }

        private void changeTheme()
        {
            Application.Current.Resources.MergedDictionaries.Clear();
            if (currentTheme == 0)
            {
                currentTheme = 1;
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/themes/Dark.xaml", UriKind.Relative) });
            }
            else if (currentTheme == 1)
            {
                currentTheme = 0;
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri("/themes/Normal.xaml", UriKind.Relative) });
            }
        }

        private void TitileBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
