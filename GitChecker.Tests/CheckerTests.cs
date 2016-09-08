using System;
using System.IO;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitChecker.Tests
{
    [TestFixture]
    public class CheckerTests
    {
        [Test]
        public void IfMaster_Pass()
        {
            var repo = CreateRepository(); //master is head
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now);
            var checker = new GitChecker(repo);

            //act
            var isValid = checker.IsValid();

            Assert.That(isValid, Is.True);
        }

        [Test]
        public void IfDevelop_Pass()
        {
            var repo = CreateRepository(); //master
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now);
            repo.CreateBranch("develop");
            repo.Checkout("develop"); //develop is head
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now);
            var checker = new GitChecker(repo);

            //act
            var isValid = checker.IsValid();

            Assert.That(isValid, Is.True);
        }

        [Test]
        public void IfCommintInDevelopToday_IsValid()
        {
            var repo = CreateRepository(); //master
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now.AddDays(-2));
            repo.CreateBranch("develop");
            repo.Checkout("develop");
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now.AddDays(-2));

            repo.CreateBranch("feature/test");
            repo.Checkout("feature/test"); //featru is head
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now.AddDays(-1));

            repo.Checkout("develop");
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now);//today

            repo.Checkout("feature/test");

            var checker = new GitChecker(repo);

            //act
            var isValid = checker.IsValid();

            Assert.That(isValid, Is.True);
        }

        [Test]
        public void IfCommintInDevelopYesterday_IsInValid()
        {
            var repo = CreateRepository(); //master
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now.AddDays(-2));
            repo.CreateBranch("develop");
            repo.Checkout("develop");
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now.AddDays(-2));

            repo.CreateBranch("feature/test");
            repo.Checkout("feature/test");
            CreateAndAddFile(repo);
            var yesterday = DateTimeOffset.Now.AddDays(-1);
            MakeCommit(repo, yesterday);

            repo.Checkout("develop");
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now.AddDays(-1));//yesterday

            repo.Checkout("feature/test");

            var checker = new GitChecker(repo);

            //act
            var isValid = checker.IsValid();

            Assert.That(isValid, Is.False);
        }

        [Test]
        public void AfterMerge_IsValid()
        {
            var repo = CreateRepository(); //master
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now.AddDays(-2));
            repo.CreateBranch("develop");
            repo.Checkout("develop");
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now.AddDays(-2));

            repo.CreateBranch("feature/test");
            repo.Checkout("feature/test");
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now.AddDays(-1));

            repo.Checkout("develop");
            CreateAndAddFile(repo);
            MakeCommit(repo, DateTimeOffset.Now); //merge

            repo.Checkout("feature/test");
            repo.Merge(repo.Branches["develop"], new Signature("test", "test@gmail.com", DateTimeOffset.Now));

            var checker = new GitChecker(repo);

            //act
            var isValid = checker.IsValid();

            Assert.That(isValid, Is.True);
        }

        private static void MakeCommit(Repository repo, DateTimeOffset dateTime)
        {
            var message = string.Format("{0} - test", repo.Head);
            repo.Commit(message, new Signature("test", "test@gmail.com", dateTime));
        }

        private void CreateAndAddFile(Repository repo)
        {
            var relativeFileName = Guid.NewGuid().ToString();
            var path = Path.Combine(repo.Info.WorkingDirectory, relativeFileName);

            var contents = Guid.NewGuid().ToString();
            File.WriteAllText(path, contents);

            repo.Stage(path);
        }

        private static Repository CreateRepository()
        {
            var path = Repository.Init(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            return new Repository(path);
        }
    }
}
