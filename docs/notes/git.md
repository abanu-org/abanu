edeeeeeee
This document contains some notes/snipptes for Arakis.

```
git remote -v
shoul be:
origin https://github.com/Arakis/MOSA-Project.git (fetch)
origin https://github.com/Arakis/MOSA-Project.git (push)
upstream https://github.com/mosa/MOSA-Project.git (fetch)
upstream https://github.com/mosa/MOSA-Project.git (push)
```

so this is the following list of commands that you enter to update your master branch, and remember, you shouldn't work on your master branch

```
git checkout master
git fetch upstream
git rebase upstream/master
git push
```

ok, lets say, i work in arakis:testing. your commands bring my arakis:master up to date. How can i get the changes from arakis:master to my arakis:testing?
A: the following commands will do the trick
```
git checkout testing
git merge master
```
but you must have committed all your code first

Signing git commits:
gpg --list-keys --fingerprint --keyid-format LONG

	# only keys with [S] can work.	
	sub   rsa4096/<key> 2019-02-11 [S]

git config --global user.signingkey <key>
git config --global commit.gpgsign true

# Windows compatibility
# per repo:
git config core.filemode false
# global:
git config --global core.filemode false
