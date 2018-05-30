using Docms.Uploader.Common;
using Docms.Uploader.FileWatch;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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
        }

        public bool CanUpload()
        {
            return SelectedMediaFiles.Any();
        }
    }
}
