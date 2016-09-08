using System;
using System.Linq;
using LibGit2Sharp;

namespace GitChecker
{
    public class GitChecker
    {
        private readonly Repository _repo;

        public GitChecker() : this(".") {}

        public GitChecker(string repoPath)
        {
            if (string.IsNullOrWhiteSpace(repoPath))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(repoPath));

            _repo = new Repository(repoPath);
        }

        //for tests
        internal GitChecker(Repository repo)
        {
            if (repo == null) throw new ArgumentNullException(nameof(repo));

            _repo = repo;
        }

        public bool IsValid()
        {
            var develop = _repo.Branches["develop"];

            var isFeature = _repo.Head.FriendlyName.Contains("feature/");
            if (!isFeature
                || develop == null
                || develop.Tip.Author.When > DateTime.Today)
            {
                return true;
            }

            return _repo.Head.Commits
                .SelectMany(c => c.Parents)
                .Contains(develop.Tip);
        }
    }
}
