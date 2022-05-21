# Changelog

## 1.0.6 (2022-05-21)

+ Fix: New-Vhdx - Fails to copy content on some systems due to automatic PSDrive creation does not occur

## 1.0.5 (2021-11-02)

+ Fix: New-Vhdx - UseShellExecute must be false for redirecting IO, fails on WinPS
+ Fix: New-Vhdx - Fails to dismount VHDX if copying of initial files & folders fails
+ Fix: New-Vhdx - Fails to copy files due to failure to identify correct volume
+ Fix: New-Vhdx - Fails to copy content due to delay in recognizing new driveletter
+ Fix: Invoke-Diskpart - Fails in JEA endpoints

## 1.0.0 (2021-05-21)

+ Initial release
