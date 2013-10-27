using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTunesLib;
using System.IO;
using System.Configuration;
using System.Threading;

namespace ItunesSync
{
    class Program
    {
        static void Main(string[] args)
        {
            var itunesSync = new ItunesSync();
            itunesSync.Sync();
        }
    }

    class ItunesSync
    {
        delegate void Router(object arg);
        iTunesApp app = new iTunesAppClass();

        string playlistName;
        string remotePath;
        string localPath;

        public ItunesSync()
        {
            playlistName = ConfigurationManager.AppSettings["SharedPlaylistName"];
            remotePath = ConfigurationManager.AppSettings["RemoteFilePath"];
            localPath = ConfigurationManager.AppSettings["LocalFilePath"];
        }

        public void Sync()
        {
            Console.WriteLine("*******************************");
            Console.WriteLine("**        ITUNES SYNC        **");
            Console.WriteLine("*******************************");
            Console.WriteLine();

            IITPlaylist playlist = null;
            foreach (IITPlaylist pl in app.LibrarySource.Playlists)
            {
                if (pl.Name == playlistName)
                {
                    playlist = pl;
                    break;
                }
            }

            //read files into library

            Console.WriteLine("Share -> Local..");

            var remoteFiles = Directory.GetFiles(remotePath);
            foreach (var file in remoteFiles)
            {
                var trackAlreadyExists = 
                    false;

                //ensure that the file hasn't already been added
                foreach (IITFileOrCDTrack playlistTrack in playlist.Tracks)
                {
                    if (Path.GetFileName(playlistTrack.Location) == Path.GetFileName(file))
                    {
                        trackAlreadyExists = true;
                        break;
                    }
                }

                if (!trackAlreadyExists)
                {
                    //this is a new file, so add it
                    var newLocation = Path.Combine(localPath, Path.GetFileName(file));
                    File.Copy(file, newLocation);
                    ((IITUserPlaylist)playlist).AddFile(newLocation);
                    Console.WriteLine("Added " + Path.GetFileName(file));
                }
            }

            Console.WriteLine("..Done!");
            Console.WriteLine();

            // write files into share

            Console.WriteLine("Local -> Share..");

            foreach (IITFileOrCDTrack playlistTrack in playlist.Tracks)
            {
                var targetPath = Path.Combine(remotePath, Path.GetFileName(playlistTrack.Location));
                if (!File.Exists(targetPath))
                {
                    File.Copy(playlistTrack.Location, targetPath);
                    Console.WriteLine("Copied " + Path.GetFileName(playlistTrack.Location));
                }
            }

            Console.WriteLine("..Done!");
            Console.WriteLine();

            Thread.Sleep(5000);
        }
    }
}
