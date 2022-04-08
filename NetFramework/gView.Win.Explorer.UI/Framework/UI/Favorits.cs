using gView.Framework.IO;
using gView.Framework.system;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace gView.Framework.UI
{
    public class MyFavorites
    {
        public MyFavorites()
        {
        }

        public bool AddFavorite(string name, string path, Image image)
        {
            try
            {
                RemoveFavorite(name, path);
                FileInfo fi = new FileInfo(ConfigPath);

                XmlStream stream = new XmlStream("Favorites");
                FavoriteList favList = new FavoriteList();
                if (fi.Exists)
                {
                    stream.ReadStream(fi.FullName);
                    favList = (FavoriteList)stream.Load("favlist", null, favList);
                    if (favList == null)
                    {
                        favList = new FavoriteList();
                    }
                }
                favList.Add(new Favorite(name, path, image));

                stream = new XmlStream("Favorites");
                stream.Save("favlist", favList);

                stream.WriteStream(fi.FullName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error - Add Favorite");
            }
            return false;
        }
        public bool RemoveFavorite(string name, string path)
        {
            try
            {
                FileInfo fi = new FileInfo(ConfigPath);
                if (!fi.Exists)
                {
                    return false;
                }

                XmlStream stream = new XmlStream("Favorites");
                stream.ReadStream(fi.FullName);
                FavoriteList favList = (FavoriteList)stream.Load("favlist", null, new FavoriteList());
                if (favList == null)
                {
                    return false;
                }

                favList.Remove(name, path);

                stream = new XmlStream("Favorites");
                stream.Save("favlist", favList);

                stream.WriteStream(fi.FullName);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error - Remove Favorite");
            }
            return false;
        }

        public List<Favorite> Favorites
        {
            get
            {
                try
                {
                    FileInfo fi = new FileInfo(ConfigPath);

                    if (fi.Exists)
                    {
                        XmlStream stream = new XmlStream("Favorites");
                        stream.ReadStream(fi.FullName);
                        return (FavoriteList)stream.Load("favlist", new FavoriteList(), new FavoriteList());
                    }
                    else
                    {
                        return new FavoriteList();
                    }
                }
                catch
                {
                    return new FavoriteList();
                }
            }
        }

        private string ConfigPath
        {
            get
            {
                return SystemVariables.MyApplicationData + @"\Favorites.xml";
            }
        }

        #region HelperClasses
        private class FavoriteList : List<Favorite>, IPersistable
        {
            public void Remove(string name, string path)
            {
                foreach (Favorite fav in ListOperations<Favorite>.Clone(this))
                {
                    if (fav.Name == name && fav.Path == path)
                    {
                        this.Remove(fav);
                    }
                }
            }

            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                Favorite fav;
                while ((fav = (Favorite)stream.Load("fav", null, new Favorite())) != null)
                {
                    this.Add(fav);
                }
            }

            public void Save(IPersistStream stream)
            {
                foreach (Favorite fav in this)
                {
                    stream.Save("fav", fav);
                }
            }

            #endregion
        }

        public class Favorite : IPersistable
        {
            string _name = String.Empty, _path = String.Empty;
            Image _image = null;

            public Favorite() { }
            public Favorite(string name, string path, Image image)
            {
                _name = name;
                _path = path;
                _image = image;
            }

            public string Name { get { return _name; } }
            public string Path { get { return _path; } }
            public Image Image { get { return _image; } }

            #region IPersistable Member

            public void Load(IPersistStream stream)
            {
                _name = (string)stream.Load("name", String.Empty);
                _path = (string)stream.Load("path", String.Empty);

                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }
                string imageBase64 = (string)stream.Load("image", String.Empty);
                if (!String.IsNullOrEmpty(imageBase64))
                {
                    try
                    {
                        _image = MyFavorites.Base64StringToImage(imageBase64);
                    }
                    catch { }
                }
            }

            public void Save(IPersistStream stream)
            {
                stream.Save("name", _name);
                stream.Save("path", _path);
                if (_image != null)
                {
                    stream.Save("image", MyFavorites.ImageToBase64String(_image, ImageFormat.Png));
                }
            }

            #endregion
        }
        #endregion

        #region Helper
        private static string ImageToBase64String(Image imageData, ImageFormat format)
        {
            string base64;
            MemoryStream memory = new MemoryStream();
            imageData.Save(memory, format);
            base64 = System.Convert.ToBase64String(memory.ToArray());
            memory.Close();
            memory = null;

            return base64;
        }
        private static Image Base64StringToImage(string base64)
        {
            MemoryStream memory = new MemoryStream(Convert.FromBase64String(base64));
            Image imageData = Image.FromStream(memory);
            memory.Close();
            memory = null;

            return imageData;
        }
        #endregion
    }

    internal class FavoriteMenuItem : ToolStripMenuItem
    {
        MyFavorites.Favorite _fav;

        public FavoriteMenuItem(MyFavorites.Favorite fav)
        {
            _fav = fav;
            if (_fav != null)
            {
                this.Text = _fav.Name;
                if (fav.Image == null)
                {
                    this.Image = global::gView.Win.Explorer.UI.Properties.Resources.folder_go;
                }
                else
                {
                    this.Image = fav.Image;
                }
            }
        }

        public MyFavorites.Favorite Favorite
        {
            get { return _fav; }
        }
    }
}
