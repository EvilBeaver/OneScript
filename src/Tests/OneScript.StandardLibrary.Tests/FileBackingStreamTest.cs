/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using OneScript.StandardLibrary.Binary;

namespace OneScript.StandardLibrary.Tests;

public class FileBackingStreamTest
{
    [Fact]
    public void CanDoBasicReadWithMemory()
    {
        using var stream = new FileBackingStream(10);
        stream.Write(new byte[]{1,2,3,4,5});

        stream.Position = 0;
        stream.Position.Should().Be(0);
        stream.HasBackingFile.Should().BeFalse();
        stream.Length.Should().Be(5);

        var dest = new byte[5];
        var size = stream.Read(dest, 0, 5);
        size.Should().Be(5);
        dest.Should().Equal(1, 2, 3, 4, 5);
    }
    
    [Fact]
    public void CanDoBasicReadWithFile()
    {
        using var stream = new FileBackingStream(10);
        stream.SwitchToFile();
        stream.Write(new byte[]{1,2,3,4,5});

        stream.Position = 0;
        stream.Position.Should().Be(0);
        stream.HasBackingFile.Should().BeTrue();
        stream.Length.Should().Be(5);

        var dest = new byte[5];
        var size = stream.Read(dest, 0, 5);
        size.Should().Be(5);
        dest.Should().Equal(1, 2, 3, 4, 5);
    }

    [Fact]
    public void GrowsOnWriteToEnd()
    {
        using var stream = new FileBackingStream(5);
        stream.Write(new byte[]{1,2,3,4,5});
        stream.HasBackingFile.Should().BeFalse();
        stream.Position.Should().Be(5);
        
        stream.Write(new byte[]{1,2,3,4,5});
        stream.HasBackingFile.Should().BeTrue();
        stream.Position.Should().Be(10);
        stream.Length.Should().Be(10);
        
        EnsureContent(stream, new byte[]{1,2,3,4,5,1,2,3,4,5});
    }
    
    [Fact]
    public void GrowsOnWriteToMiddle()
    {
        using var stream = new FileBackingStream(5);
        stream.Write(new byte[]{1,2,3,4,5});
        stream.Position = 2;
        stream.HasBackingFile.Should().BeFalse();
        
        stream.Write(new byte[]{6,6,6,6,6});
        stream.HasBackingFile.Should().BeTrue();
        stream.Position.Should().Be(7);
        stream.Length.Should().Be(7);
        
        EnsureContent(stream, new byte[]{1,2,6,6,6,6,6});
    }
    
    [Fact]
    public void GrowsOnWriteToStart()
    {
        using var stream = new FileBackingStream(5);
        stream.Write(new byte[]{1,2,3,4,5});
        stream.Position = 0;
        stream.HasBackingFile.Should().BeFalse();
        
        stream.Write(new byte[]{7,6,5,4,3,2,1});
        stream.HasBackingFile.Should().BeTrue();
        stream.Position.Should().Be(7);
        stream.Length.Should().Be(7);
        
        EnsureContent(stream, new byte[]{7,6,5,4,3,2,1});
    }
    
    [Fact]
    public void DoesNotGrowOnWriteInsideLimit()
    {
        using var stream = new FileBackingStream(5);
        stream.Write(new byte[]{1,2,3,4,5});
        stream.Position = 2;
        stream.HasBackingFile.Should().BeFalse();
        
        stream.Write(new byte[]{0,0,0});
        stream.HasBackingFile.Should().BeFalse();
        stream.Position.Should().Be(5);
        stream.Length.Should().Be(5);
        
        EnsureContent(stream, new byte[]{1,2,0,0,0});
    }
    
    [Fact]
    public void CanCropIntoMemoryAgain()
    {
        using var stream = new FileBackingStream(5);
        stream.Write(new byte[]{1,2,3,4,5,6,7,8,9,10});
        stream.HasBackingFile.Should().BeTrue();
        stream.Length.Should().Be(10);
        stream.SetLength(5);
        stream.HasBackingFile.Should().BeFalse();
        
        EnsureContent(stream, new byte[]{1,2,3,4,5});
    }
    
    [Fact]
    public void SettingLengthBeyondLimitSwitchesToFile()
    {
        using var stream = new FileBackingStream(5);
        stream.HasBackingFile.Should().BeFalse();
        stream.SetLength(15);
        stream.HasBackingFile.Should().BeTrue();
    }
    
    [Fact]
    public void ThrowsWhenCantSwitchIntoMemory()
    {
        using var stream = new FileBackingStream(5);
        stream.Write(new byte[]{1,2,3,4,5,6,7,8,9,10});
        stream.HasBackingFile.Should().BeTrue();
        
        Assert.Throws<InvalidOperationException>(() => stream.SwitchToMemory());
    }

    [Fact]
    public void SetPositionBeyondLength()
    {
        using var stream = new FileBackingStream(5);
        stream.Position = 10;
        
        stream.Length.Should().Be(0);
        stream.HasBackingFile.Should().BeFalse();
        
        stream.Write(new byte[]{1,2,3});
        stream.Position.Should().Be(13);
        stream.Length.Should().Be(13);
        stream.HasBackingFile.Should().BeTrue();
    }

    private void EnsureContent(Stream stream, IEnumerable<byte> expected)
    {
        stream.Position = 0;
        var actual = new byte[stream.Length];
        stream.Read(actual).Should().Be((int)stream.Length);

        actual.Should().Equal(expected);
    }
    
    [Fact(Skip = "Platform test")]
    public void SetPositionBeyondMemoryStream()
    {
        using var ms = new MemoryStream();
        ms.Position = 10;
        
        ms.Length.Should().Be(0);
        ms.Write(new byte[]{1,2,3});
        ms.Position.Should().Be(13);
        ms.Length.Should().Be(13);
    }
    
    [Fact(Skip = "Platform test")]
    public void SetPositionBeyondFileStream()
    {
        var path = Path.GetTempFileName();
        var fs = new FileStream(path, FileMode.Create);
        try
        {
            fs.Position = 10;

            fs.Length.Should().Be(0);
            fs.Write(new byte[] { 1, 2, 3 });
            fs.Position.Should().Be(13);
            fs.Length.Should().Be(13);
        }
        finally
        {
            fs.Close();
            File.Delete(path);
        }
    }
}