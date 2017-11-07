# Protobuild.Bootstrap

Bootstrap is designed to be a lightweight wrapper for protobuild.exe, allowing you to update the version of protobuild in your repository without needing to deal with storing and merging lots of binary blobs in your repository.

# How to use

Place Bootstrap's protobuild.exe in the root of your repository, much like you would a normal Protobuild instalation.  Then, simply set the `ProtobuildVersion` value of your Module.xml to the desired version hash.

To update Protobuild, simply bump the hash.  No replacing of binary files required!