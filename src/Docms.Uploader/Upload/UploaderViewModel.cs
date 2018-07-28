using Docms.Client;
using Docms.Client.Models;
using Docms.Uploader.Common;
using Docms.Uploader.FileWatch;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Uploader.Upload
{
    public class UploaderViewModel : ViewModelBase
    {
        private CustomerResponse _Customer;
        private ProjectResponse _Project;
        private UserResponse _PersonInCharge;
        private string _CustomerText;
        private string _ProjectText;
        private string _PersonInChargeText;
        private bool _Loading;
        private DocmsClient _client;

        public class MediaFilesCollection : ObservableCollection<WatchingFileViewModel> { }
        public class TagsCollection : ObservableCollection<Tag> { }


        public MediaFilesCollection SelectedMediaFiles { get; }

        public ObservableCollection<CustomerResponse> CustomerChoices { get; } = new ObservableCollection<CustomerResponse>();
        public ObservableCollection<ProjectResponse> ProjectChoices { get; } = new ObservableCollection<ProjectResponse>();
        public ObservableCollection<UserResponse> PersonInChargeChoices { get; } = new ObservableCollection<UserResponse>();

        public CustomerResponse Customer
        {
            get { return _Customer; }
            set
            {
                SetProperty(ref _Customer, value);
                Project = null;
                ProjectChoices.Clear();
                value.Projects.ToList().ForEach(p =>
                {
                    if (p.CustomerId == value.Id)
                    {
                        ProjectChoices.Add(p);
                    }
                });
            }
        }

        public ProjectResponse Project
        {
            get { return _Project; }
            set
            {
                SetProperty(ref _Project, value);
            }
        }

        public UserResponse PersonInCharge
        {
            get { return _PersonInCharge; }
            set
            {
                SetProperty(ref _PersonInCharge, value);
            }

        }
        public string CustomerText
        {
            get { return _CustomerText; }
            set
            {
                SetProperty(ref _CustomerText, value);
            }
        }

        public string ProjectText
        {
            get { return _ProjectText; }
            set
            {
                SetProperty(ref _ProjectText, value);
            }
        }

        public string PersonInChargeText
        {
            get { return _PersonInChargeText; }
            set
            {
                SetProperty(ref _PersonInChargeText, value);
            }

        }

        public TagsCollection TagChoices { get; } = new TagsCollection();
        public TagsCollection SelectedTags { get; set; }
        public bool Loading
        {
            get { return _Loading; }
            set
            {
                SetProperty(ref _Loading, value);
            }

        }

        public UploaderViewModel(DocmsClient client)
        {
            _client = client;
            SelectedMediaFiles = new MediaFilesCollection();
            SelectedTags = new TagsCollection();
            Loading = true;
            InitializeAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    Loading = false;
                }
            });
        }

        private async Task InitializeAsync()
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            await _client.VerifyTokenAsync().ConfigureAwait(false);
            var tasks = new List<Task>();
            tasks.Add(_client.GetCustomersAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    foreach (var c in t.Result)
                    {
                        CustomerChoices.Add(c);
                    }
                }
            }, scheduler));
            tasks.Add(_client.GetUsersAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    foreach (var u in t.Result)
                    {
                        PersonInChargeChoices.Add(u);
                    }
                }
            }, scheduler));
            tasks.Add(_client.GetTagsAsync().ContinueWith(t =>
            {
                if (t.IsCompleted)
                {
                    foreach (var tag in t.Result)
                    {
                        TagChoices.Add(new Tag(tag.Id, tag.Name));
                    }
                }
            }, scheduler));
            await Task.WhenAll(tasks);
        }

        public void SelectFile(WatchingFileViewModel file)
        {
            var index = SelectedMediaFiles.IndexOf(file);
            if (index < 0)
            {
                SelectedMediaFiles.Add(file);
            }
        }

        public async Task Upload()
        {
            await _client.VerifyTokenAsync().ConfigureAwait(false);
            var tags = SelectedTags.Select(t => t.Name).ToList();
            foreach (var f in SelectedMediaFiles)
            {
                await _client.CreateDocumentAsync(f.FullPath,
                    f.FileName,
                    PersonInCharge?.Name ?? PersonInChargeText,
                    Customer?.Name ?? CustomerText,
                    Project?.Name ?? ProjectText,
                    tags.ToArray());
            }
        }

        public bool CanUpload()
        {
            return SelectedMediaFiles.Any();
        }
    }
}
