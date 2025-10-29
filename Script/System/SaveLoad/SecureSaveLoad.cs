// SecureSaveLoad.cs
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using Steamworks;

public static class SecureSaveLoad
{
    private const string KEY_FILE_NAME = "user_encryption_key.dat";
    private static byte[] _encryptionKeyBytes = null;

    // 📢 추가: 자동 클라우드 동기화 옵션
    private const bool AUTO_CLOUD_SYNC = true;

    /// <summary>
    /// 사용자별 암호화 키를 생성하고 저장 (최초 실행 시)
    /// </summary>
    private static bool GenerateAndSaveNewKey()
    {
        _encryptionKeyBytes = new byte[16];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(_encryptionKeyBytes);
        }

        if (SteamRemoteStorage.FileWrite(KEY_FILE_NAME, _encryptionKeyBytes, _encryptionKeyBytes.Length))
        {
            Debug.Log("[SecureSave] 새로운 고유 암호화 키를 생성하고 스팀 클라우드에 저장했습니다.");
            return true;
        }
        else
        {
            Debug.LogError("[SecureSave] 고유 암호화 키를 스팀 클라우드에 저장하는데 실패했습니다.");
            _encryptionKeyBytes = null;
            return false;
        }
    }

    /// <summary>
    /// 사용자별 암호화 키를 스팀 클라우드에서 로드
    /// </summary>
    private static bool LoadKey()
    {
        int fileSize = SteamRemoteStorage.GetFileSize(KEY_FILE_NAME);

        if (fileSize <= 0)
        {
            return GenerateAndSaveNewKey();
        }

        _encryptionKeyBytes = new byte[fileSize];
        if (SteamRemoteStorage.FileRead(KEY_FILE_NAME, _encryptionKeyBytes, fileSize) == fileSize)
        {
            Debug.Log("[SecureSave] 스팀 클라우드에서 고유 암호화 키를 성공적으로 로드했습니다.");
            return true;
        }
        else
        {
            Debug.LogError("[SecureSave] 스팀 클라우드에서 고유 암호화 키를 읽는데 실패했습니다.");
            _encryptionKeyBytes = null;
            return false;
        }
    }

    /// <summary>
    /// XOR 암호화/복호화
    /// </summary>
    private static byte[] EncryptDecrypt(byte[] data)
    {
        if (_encryptionKeyBytes == null)
        {
            if (!LoadKey())
            {
                Debug.LogError("[SecureSave] 암호화 키가 없습니다. 암호화/복호화 실패.");
                return data;
            }
        }

        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            result[i] = (byte)(data[i] ^ _encryptionKeyBytes[i % _encryptionKeyBytes.Length]);
        }
        return result;
    }

    /// <summary>
    /// SHA256 해시 생성
    /// </summary>
    private static string CalculateHash(string json)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// 파일 저장: 암호화 및 해시 포함
    /// </summary>
    public static void SaveData(string filePath, GlobalSaveData dataToSave)
    {
        // 0. 암호화 키 로드/생성 확인
        if (_encryptionKeyBytes == null && !LoadKey())
        {
            Debug.LogError("[SecureSave] 세이브 데이터를 저장할 수 없습니다. 암호화 키가 없습니다.");
            return;
        }

        // 1. 데이터에 해시 생성 및 삽입
        dataToSave.integrityHash = string.Empty;
        string dataJson = JsonUtility.ToJson(dataToSave);
        dataToSave.integrityHash = CalculateHash(dataJson);

        // 2. 해시가 포함된 최종 JSON을 암호화
        string finalJson = JsonUtility.ToJson(dataToSave);
        byte[] encryptedBytes = EncryptDecrypt(Encoding.UTF8.GetBytes(finalJson));

        // 3. 로컬 파일 저장
        try
        {
            // 디렉토리가 없으면 생성
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(filePath, encryptedBytes);
            Debug.Log($"[SecureSave] 로컬 저장 완료: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SecureSave] 로컬 저장 실패: {e.Message}");
            return;
        }

        // 📢 수정: Steam Cloud 자동 동기화
        if (AUTO_CLOUD_SYNC && SteamManager.Initialized)
        {
            // 파일 이름만 추출 (전체 경로가 아닌)
            string fileName = Path.GetFileName(filePath);
            SteamCloudManager.InitiateCloudSave(fileName, encryptedBytes);
        }
    }

    /// <summary>
    /// 파일 로드: 복호화 및 무결성 검증
    /// 로컬 파일이 없으면 Steam Cloud에서 다운로드 시도
    /// </summary>
    public static GlobalSaveData LoadData(string filePath)
    {
        // 0. 암호화 키 로드/생성 확인
        if (_encryptionKeyBytes == null && !LoadKey())
        {
            Debug.LogError("[SecureSave] 세이브 데이터를 로드할 수 없습니다. 암호화 키가 없습니다.");
            return null;
        }

        byte[] encryptedBytes = null;

        // 1. 로컬 파일 확인
        if (File.Exists(filePath))
        {
            encryptedBytes = File.ReadAllBytes(filePath);
            Debug.Log($"[SecureSave] 로컬 파일 로드: {filePath}");
        }
        // 📢 추가: 로컬에 없으면 Steam Cloud에서 다운로드 시도
        else if (SteamManager.Initialized)
        {
            string fileName = Path.GetFileName(filePath);
            int fileSize = SteamRemoteStorage.GetFileSize(fileName);

            if (fileSize > 0)
            {
                encryptedBytes = new byte[fileSize];
                int bytesRead = SteamRemoteStorage.FileRead(fileName, encryptedBytes, fileSize);

                if (bytesRead == fileSize)
                {
                    Debug.Log($"[SecureSave] Steam Cloud에서 파일 다운로드 성공: {fileName}");

                    // 로컬에도 저장 (캐싱)
                    try
                    {
                        string directory = Path.GetDirectoryName(filePath);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                        File.WriteAllBytes(filePath, encryptedBytes);
                        Debug.Log($"[SecureSave] 로컬 캐시 저장 완료: {filePath}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[SecureSave] 로컬 캐시 저장 실패 (로드는 계속): {e.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"[SecureSave] Steam Cloud 다운로드 실패: {fileName}");
                    return null;
                }
            }
            else
            {
                Debug.Log($"[SecureSave] 저장된 파일이 없습니다: {filePath}");
                return null;
            }
        }
        else
        {
            Debug.Log($"[SecureSave] 저장된 파일이 없습니다: {filePath}");
            return null;
        }

        // 2. 복호화
        byte[] decryptedBytes = EncryptDecrypt(encryptedBytes);
        string finalJson = Encoding.UTF8.GetString(decryptedBytes);

        GlobalSaveData loadedData;
        try
        {
            loadedData = JsonUtility.FromJson<GlobalSaveData>(finalJson);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SecureSave] JSON 파싱 실패: {e.Message}");
            return null;
        }

        // 3. 무결성 검증
        string originalHash = loadedData.integrityHash;
        loadedData.integrityHash = string.Empty;
        string dataJsonForCheck = JsonUtility.ToJson(loadedData);
        string newHash = CalculateHash(dataJsonForCheck);

        if (newHash != originalHash)
        {
            Debug.LogError("[SecureSave] 파일 무결성 검증 실패! 파일이 변조되었거나 교체되었습니다.");
            return null;
        }

        // 4. 검증 성공 시 원본 해시 값 복원 및 데이터 반환
        loadedData.integrityHash = originalHash;
        Debug.Log("[SecureSave] 파일 로드 및 무결성 검증 성공!");
        return loadedData;
    }

    /// <summary>
    /// 저장 파일 삭제 (로컬 + Steam Cloud)
    /// </summary>
    public static void DeleteSaveData(string filePath)
    {
        // 로컬 파일 삭제
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"[SecureSave] 로컬 파일 삭제: {filePath}");
        }

        // Steam Cloud 파일 삭제
        if (SteamManager.Initialized)
        {
            string fileName = Path.GetFileName(filePath);
            if (SteamRemoteStorage.FileExists(fileName))
            {
                SteamRemoteStorage.FileDelete(fileName);
                Debug.Log($"[SecureSave] Steam Cloud 파일 삭제: {fileName}");
            }
        }
    }
}