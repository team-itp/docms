using Docms.Api;
using Docms.Uploader.Common;
using Docms.Uploader.FileWatch;
using Docms.Uploader.Properties;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API = Docms.Api.Models;

namespace Docms.Uploader.Upload
{
    public class UploaderViewModel : ViewModelBase
    {
        public class MediaFilesCollection : ObservableCollection<MediaFile> { }
        public class TagsCollection : ObservableCollection<Tag> { }

        public MediaFilesCollection SelectedMediaFiles { get; }

        public TagsCollection ProjectChoices { get; set; }
        public Tag Project { get; set; }
        public TagsCollection TagChoices { get; set; }
        public TagsCollection SelectedTags { get; set; }

        private static DocmsClient apiClient = new DocmsClient("http://localhost/api", "v1");

        public UploaderViewModel()
        {
            SelectedMediaFiles = new MediaFilesCollection();
            SelectedTags = new TagsCollection();
            ProjectChoices = new TagsCollection();
            ProjectChoices.Add(new Tag("案件1"));
            ProjectChoices.Add(new Tag("案件2"));
            ProjectChoices.Add(new Tag("案件3"));

            TagChoices = new TagsCollection();
            TagChoices.Add(new Tag("フリーのタグ1"));
            TagChoices.Add(new Tag("フリーのタグ2"));
            TagChoices.Add(new Tag("フリーのタグ3"));
        }

        public void SelectFile(MediaFile file)
        {
            var index = SelectedMediaFiles.IndexOf(file);
            if (index < 0)
            {
                SelectedMediaFiles.Add(file);
            }
        }

        public async Task Upload()
        {
            var userId = Settings.Default.UserId;
            var password = Settings.Default.PasswordHash;
            await apiClient.RequestJWToken(userId, password);
            foreach (var t in SelectedTags
                .Where(tag => tag.Id == -1)
                .ToList())
            {
                var createdTag = await apiClient.CreateTag(new API.Tag(t.Name));
                t.UpdateId(createdTag.Id);
            }

            var tags = SelectedTags.Select(st => st.TermId).ToArray();
            foreach (var file in this.SelectedMediaFiles)
            {
                var newPost = new API.Document();
                newPost.Title = file.Name;
                newPost.Tags = SelectedTags.Select(st => st.TermId).ToArray();
                var stream = new FileStream(file.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    var post = await apiClient.CreatePost(newPost, stream);
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        public bool CanUpload()
        {
            return SelectedMediaFiles.Any();
        }
    }
}
