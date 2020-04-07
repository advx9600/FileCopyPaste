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

        String pasteKey = "";
        String refreshTree0Key = "";
        String delKey = "";
        String enterKey = "";
        String newFolderKey = "";

        List<DataOp.OpenFileApp> openFileAppKeys;

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
            pasteKey = readKey("key_paste");
            refreshTree0Key = readKey("key_refresh_tree0");
            delKey = readKey("key_delete");
            enterKey = readKey("key_enter");
            newFolderKey = readKey("key_new_folder");
            openFileAppKeys = DataOp.ReadOpenFileApps();
        }

        private void readPath()
        {            
            for(var i = 0; i < paths.Length; i++)
            {
                paths[i] = readKey(String.Format("path{0}",i));
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var num = 0;
            if (sender == Btn2) num=1;

            var dialog = new FolderSelectDialog();
            dialog.InitialDirectory = paths[num];
            {
                if (!paths[num].Equals(dialog.FileName))
                {
                    paths[num] = dialog.FileName;
                    writeKey(String.Format("path{0}",num), paths[num]);

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
            var num = 0;
            if (tree == Tree1) num = 1;
            else if (tree == Tree2) num = 2;

            resetTree(num);
            expandTree(tree.Items,selectedItem[num]);
        }

        private void expandTree(ItemCollection items,TreeViewItem target)
        {
            if (items != null && target != null && target.Tag != null)
            {
                var id = (target.Tag as DirectoryInfo).FullName;                
                foreach(var itemsingle in items)
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
        private List<TreeViewItem> getDirs(string totaldir,int loopNum,bool setTotalDir = false)
        {
            List<TreeViewItem> list = new List<TreeViewItem>();
            var dirinfo = new DirectoryInfo(totaldir);
            if (loopNum > 0) { // 最多N层，太多没有必要
                try {
                    var dirs = dirinfo.GetDirectories();
                    if (setTotalDir)
                    {
                        dirs = new DirectoryInfo[1] { dirinfo };
                    }

                    foreach (var dir in dirs)
                    {
                        if (dir.Name.StartsWith(".")) continue;                    

                        TreeViewItem item = new TreeViewItem();
                        item.Header = dir.Name;
                        item.Tag = dir;
                        list.Add(item);

                        var list2 = getDirs(dir.FullName,loopNum -1);
                        if (list2.Count() > 0) { 
                            foreach(var subitem in list2)
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

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;
            var item = tree.SelectedItem as TreeViewItem;
            if (item == null) return;

            var num = sender == Tree0 ?0: 1;
            if (sender == Tree2) num = 2;
            
            var path = getItemPath(item);

            selectedItem[num] = item;

            if ( num > 0)
            {
                ListFile.Items.Clear();
                selectedFile = null;
                var dirinfo = new DirectoryInfo(path);
                foreach(var fileinfo in dirinfo.GetFiles())
                {
                    if (fileinfo.Name.Equals("desktop.ini"))
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
            if (item != null) { 
                var fileinfo = item.Tag as FileInfo;
                selectedFile = fileinfo;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (isKeyDown(e, pasteKey))
            {
                var message = "";
                if (selectedFile != null && selectedItem[0] != null)
                {
                    String pasteName = System.IO.Path.Combine(getItemPath(selectedItem[0]),selectedFile.Name);
                    if (File.Exists(pasteName))
                    {
                        File.Delete(pasteName);
                    }
                    File.Copy(selectedFile.FullName, pasteName);
                    message =selectedFile.FullName+Environment.NewLine+"copy to"+Environment.NewLine+ pasteName;
                }
                else
                {
                    message = "no file or directory selected";
                }

                MessageBox.Show(message);
            }else if (isKeyDown(e, refreshTree0Key))
            {
                refreshTree(Tree0);
            }else if (isKeyDown(e, delKey))
            {
                if (isListViewItemFocused(ListFile.Items) && selectedFile != null)
                {
                    File.Delete(selectedFile.FullName);
                    ListFile.Items.Remove(ListFile.SelectedItem);
                }
                else
                {
                    TreeView tree = null;
                    if (isTreeViewItemFocused(Tree0.Items)) tree = Tree0;
                    else if (isTreeViewItemFocused(Tree1.Items)) tree = Tree1;
                    else if (isTreeViewItemFocused(Tree2.Items)) tree = Tree2;
                    if (tree.SelectedItem != null)
                    {
                        var info = (tree.SelectedItem as TreeViewItem).Tag as DirectoryInfo;
                        if (MessageBox.Show("delete "+ info.Name+" ?") == MessageBoxResult.OK)
                        {
                            try { 
                                MyUtils.delDirecotry(info);
                                refreshTree(tree);
                            }catch(Exception e2)
                            {
                                MessageBox.Show("删除失败，最多只能删除2层文件");
                            }
                        }
                    }
                }
            }else if (isKeyDown(e, enterKey))
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
            }else if (isKeyDown(e, newFolderKey))
            {
                var tree = Tree0;
                if (isTreeViewItemFocused(Tree1.Items)) tree = Tree1;
                else if (isTreeViewItemFocused(Tree2.Items)) tree = Tree2;

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
                            Directory.CreateDirectory(System.IO.Path.Combine(info,name));
                            refreshTree(tree);
                        }
                    }
                }
            }
        }

        private bool isTreeViewItemFocused(ItemCollection items)
        {
            bool focused = false;
            if (items != null)
                foreach(var item in items)
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
                foreach(var it in items)
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
        private bool isKeyDown(KeyEventArgs e,string key)
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
                    }else if (splitkey.Equals("shift"))
                    {
                        shiftkey = true;
                    }else if (splitkey.Equals("alt"))
                    {
                        altkey = true;
                    }
                    else
                    {
                        actualkey = splitkey;
                    }
                }

                if (ctrlkey == ((Keyboard.Modifiers & ModifierKeys.Control)== ModifierKeys.Control) &&
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
            if (tree != Tree0)
            {
                if (tree.SelectedItem != null)
                {
                    Tree_SelectedItemChanged(sender, null);
                }
            }
        }
    }
}
