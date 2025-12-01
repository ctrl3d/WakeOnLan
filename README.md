# WakeOnLan for Unity

[![Unity 2022.1+](https://img.shields.io/badge/unity-2022.1%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/github/license/ctrl3d/WakeOnLan)](https://github.com/ctrl3d/WakeOnLan/blob/main/LICENSE)
[![openupm](https://img.shields.io/npm/v/work.ctrl3d.wake-on-lan?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/work.ctrl3d.wake-on-lan/)

[English](README_EN.md) | 한국어

Unity에서 Wake-on-LAN (WOL) 매직 패킷을 전송하여 원격 컴퓨터를 깨울 수 있는 라이브러리입니다.

## 기능

- 비동기 매직 패킷 전송 (async/await)
- 브로드캐스트 또는 특정 서브넷으로 패킷 전송
- UniTask 지원 (선택 사항)
- Unity 2022.1 이상 지원

## 설치

### Unity Package Manager를 통한 설치

1. Unity 에디터에서 `Window` → `Package Manager` 를 엽니다
2. 좌측 상단의 `+` 버튼을 클릭합니다
3. `Add package from git URL...` 을 선택합니다
4. 다음 URL을 입력합니다:
   ```
   https://github.com/ctrl3d/WakeOnLan.git?path=Assets/WakeOnLan
   ```

### manifest.json을 통한 설치

프로젝트의 `Packages/manifest.json` 파일을 열고 `dependencies` 항목에 다음을 추가합니다:

```json
{
  "dependencies": {
    "work.ctrl3d.wake-on-lan": "https://github.com/ctrl3d/WakeOnLan.git?path=Assets/WakeOnLan"
  }
}
```

## 사용법

### 기본 사용법 (비동기)

```csharp
using work.ctrl3d;
using UnityEngine;

public class WakeOnLanExample : MonoBehaviour
{
    async void Start()
    {
        try
        {
            // 브로드캐스트로 매직 패킷 전송
            await WakeOnLan.SendMagicPacketAsync("AA:BB:CC:DD:EE:FF");
            Debug.Log("매직 패킷 전송 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"매직 패킷 전송 실패: {ex.Message}");
        }
    }
}
```

### 동기식 사용법

```csharp
using work.ctrl3d;
using UnityEngine;

public class WakeOnLanSync : MonoBehaviour
{
    void Start()
    {
        try
        {
            // 동기식으로 매직 패킷 전송 (메인 스레드 블로킹)
            WakeOnLan.SendMagicPacket("AA:BB:CC:DD:EE:FF");
            Debug.Log("매직 패킷 전송 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"매직 패킷 전송 실패: {ex.Message}");
        }
    }
}
```

**참고:** 동기식 메서드는 패킷이 전송될 때까지 메인 스레드를 블로킹합니다. 가능하면 비동기 메서드 사용을 권장합니다.

### 고급 옵션

#### 특정 IP로 유니캐스트 전송

```csharp
// IP 주소만 지정 (서브넷 마스크 없음) - 해당 IP로 직접 전송
await WakeOnLan.SendMagicPacketAsync(
    macAddress: "AA:BB:CC:DD:EE:FF",
    ipAddress: "192.168.1.100"
);
```

#### 서브넷 브로드캐스트 주소로 전송

```csharp
// IP 주소와 서브넷 마스크 지정 - 서브넷 브로드캐스트 주소 계산 (예: 192.168.1.255)
await WakeOnLan.SendMagicPacketAsync(
    macAddress: "AA:BB:CC:DD:EE:FF",
    ipAddress: "192.168.1.100",
    subnetMask: "255.255.255.0"
);
```

**요약:**
- **MAC만**: 전체 브로드캐스트 (255.255.255.255) - 대부분의 경우 이것만으로 충분
- **MAC + IP**: 특정 IP로 유니캐스트 전송
- **MAC + IP + 서브넷**: 계산된 서브넷 브로드캐스트 주소로 전송

### MAC 주소 형식

다음과 같은 형식의 MAC 주소를 지원합니다:

- `AA:BB:CC:DD:EE:FF` (콜론 구분)
- `AA-BB-CC-DD-EE-FF` (하이픈 구분)
- `AABBCCDDEEFF` (구분자 없음)

### UniTask 지원

UniTask를 사용하는 경우, 패키지의 Version Defines 기능을 통해 자동으로 `UniTask`를 사용합니다.

## API 참조

### SendMagicPacket (동기식)

```csharp
public static void SendMagicPacket(
    string macAddress,
    string ipAddress = null,
    string subnetMask = null,
    int port = 9
)
```

**매개변수:**
- `macAddress` (string): 대상 컴퓨터의 MAC 주소
- `ipAddress` (string, 선택): IP 주소
  - 미지정: 전체 브로드캐스트 (255.255.255.255)
  - 지정 (서브넷 마스크 없음): 해당 IP로 유니캐스트 전송
  - 지정 (서브넷 마스크 있음): 서브넷 브로드캐스트 주소로 전송
- `subnetMask` (string, 선택): 서브넷 마스크 (IP 주소와 함께 지정 시 브로드캐스트 주소 계산)
- `port` (int, 선택): WOL 포트 번호 (기본값: 9)

**예외:**
- `FormatException`: MAC 주소, IP 주소, 또는 서브넷 마스크 형식이 잘못된 경우
- `InvalidOperationException`: 패킷 전송 실패 시

**주의:** 이 메서드는 동기식으로 실행되어 호출 스레드를 블로킹합니다.

### SendMagicPacketAsync (비동기)

```csharp
public static async Task SendMagicPacketAsync(
    string macAddress,
    string ipAddress = null,
    string subnetMask = null,
    int port = 9
)
```

**매개변수:**
- `macAddress` (string): 대상 컴퓨터의 MAC 주소
- `ipAddress` (string, 선택): IP 주소
  - 미지정: 전체 브로드캐스트 (255.255.255.255)
  - 지정 (서브넷 마스크 없음): 해당 IP로 유니캐스트 전송
  - 지정 (서브넷 마스크 있음): 서브넷 브로드캐스트 주소로 전송
- `subnetMask` (string, 선택): 서브넷 마스크 (IP 주소와 함께 지정 시 브로드캐스트 주소 계산)
- `port` (int, 선택): WOL 포트 번호 (기본값: 9)

**반환값:**
- `Task` (또는 USE_UNITASK 정의 시 `UniTask`)

**예외:**
- `FormatException`: MAC 주소, IP 주소, 또는 서브넷 마스크 형식이 잘못된 경우
- `InvalidOperationException`: 패킷 전송 실패 시

## 플랫폼 호환성

| 플랫폼 | 지원 여부 | 비고 |
|--------|-----------|------|
| Windows | ✅ | 완전 지원 |
| macOS | ✅ | 완전 지원 |
| Linux | ✅ | 완전 지원 |
| Android | ✅ | 네트워크 권한 필요 |
| iOS | ✅ | 네트워크 권한 필요 |
| WebGL | ❌ | UDP 소켓 미지원 |

## 고급 사용 예제

### UI 버튼과 연동

```csharp
using work.ctrl3d;
using UnityEngine;
using UnityEngine.UI;

public class WakeOnLanButton : MonoBehaviour
{
    [SerializeField] private Button wakeButton;
    [SerializeField] private string targetMacAddress = "AA:BB:CC:DD:EE:FF";

    void Start()
    {
        wakeButton.onClick.AddListener(OnWakeButtonClicked);
    }

    async void OnWakeButtonClicked()
    {
        wakeButton.interactable = false;

        try
        {
            await WakeOnLan.SendMagicPacketAsync(targetMacAddress);
            Debug.Log("PC 깨우기 성공!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"실패: {ex.Message}");
        }
        finally
        {
            wakeButton.interactable = true;
        }
    }
}
```

### 여러 컴퓨터 순차 깨우기

```csharp
using work.ctrl3d;
using UnityEngine;
using System.Collections.Generic;

public class MultipleWakeOnLan : MonoBehaviour
{
    [System.Serializable]
    public class WakeTarget
    {
        public string name;
        public string macAddress;
        public string ipAddress;
        public string subnetMask;
    }

    [SerializeField] private List<WakeTarget> targets;

    public async void WakeAllComputers()
    {
        foreach (var target in targets)
        {
            try
            {
                Debug.Log($"{target.name} 깨우는 중...");

                if (string.IsNullOrEmpty(target.ipAddress))
                {
                    await WakeOnLan.SendMagicPacketAsync(target.macAddress);
                }
                else
                {
                    await WakeOnLan.SendMagicPacketAsync(
                        target.macAddress,
                        target.ipAddress,
                        target.subnetMask
                    );
                }

                Debug.Log($"{target.name} 완료!");

                // 각 패킷 사이 1초 대기
                await System.Threading.Tasks.Task.Delay(1000);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{target.name} 실패: {ex.Message}");
            }
        }
    }
}
```

### 에러 처리 베스트 프랙티스

```csharp
using work.ctrl3d;
using UnityEngine;
using System;

public class WakeOnLanWithErrorHandling : MonoBehaviour
{
    async void WakeComputerSafely(string macAddress, string ip = null, string subnet = null)
    {
        try
        {
            await WakeOnLan.SendMagicPacketAsync(macAddress, ip, subnet);
            Debug.Log("매직 패킷 전송 완료");
        }
        catch (FormatException ex)
        {
            // MAC 주소나 IP 주소 형식 오류
            Debug.LogError($"주소 형식 오류: {ex.Message}");
            // UI에 사용자 친화적인 메시지 표시
        }
        catch (ArgumentNullException ex)
        {
            // IP는 있는데 서브넷 마스크가 없는 경우
            Debug.LogError($"필수 매개변수 누락: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            // 네트워크 전송 실패
            Debug.LogError($"네트워크 오류: {ex.Message}");
            // 네트워크 연결 확인 요청
        }
        catch (Exception ex)
        {
            // 기타 예외
            Debug.LogError($"알 수 없는 오류: {ex.Message}");
        }
    }
}
```

## 트러블슈팅

### 매직 패킷이 전송되지 않아요

1. **방화벽 확인**
   - Windows: Windows Defender 방화벽에서 UDP 포트 9 허용
   - macOS: 시스템 환경설정 → 보안 및 개인정보 보호 → 방화벽
   - 아웃바운드 UDP 트래픽 허용 확인

2. **네트워크 권한 (모바일)**
   - Android: `AndroidManifest.xml`에 인터넷 권한 추가
     ```xml
     <uses-permission android:name="android.permission.INTERNET" />
     ```
   - iOS: `Info.plist`에 네트워크 권한 설정

3. **라우터 설정**
   - 대상 컴퓨터와 같은 네트워크에 있는지 확인
   - 원격 네트워크의 경우 라우터에서 UDP 포트 9 포워딩 설정

### WebGL에서 작동하지 않아요

WebGL은 브라우저 보안 제약으로 UDP 소켓을 지원하지 않습니다. 대안으로 서버를 통한 우회 방식을 사용해야 합니다.

### MAC 주소 형식 오류가 발생해요

다음 형식만 지원됩니다:
- `AA:BB:CC:DD:EE:FF` (콜론)
- `AA-BB-CC-DD-EE-FF` (하이픈)
- `AABBCCDDEEFF` (구분자 없음)

대소문자는 구분하지 않습니다.

### 대상 컴퓨터가 깨어나지 않아요

WOL 매직 패킷 전송만으로는 충분하지 않을 수 있습니다:

1. **대상 PC BIOS/UEFI 설정**
   - Wake-on-LAN 기능 활성화 필요
   - "Power On by PCI-E/PCIe" 옵션 활성화

2. **네트워크 어댑터 설정 (Windows)**
   - 장치 관리자 → 네트워크 어댑터
   - 속성 → 전원 관리
   - "Magic Packet에서 컴퓨터의 대기 모드 해제 허용" 체크

3. **완전 종료 vs 절전 모드**
   - Windows의 빠른 시작 기능은 WOL과 호환되지 않을 수 있음
   - 대상 PC를 절전 모드나 최대 절전 모드로 전환 권장

## 보안 및 성능 참고사항

### 보안

- **네트워크 권한**: 이 라이브러리는 UDP 소켓을 사용하므로 인터넷 권한이 필요합니다
- **브로드캐스트**: 기본적으로 브로드캐스트를 사용하므로 같은 네트워크의 모든 장치가 패킷을 수신할 수 있습니다
- **인증 없음**: Wake-on-LAN 프로토콜 자체는 인증 메커니즘이 없으므로, 중요한 시스템의 경우 추가 보안 계층을 구현하세요

### 성능

- **경량**: 매직 패킷은 102바이트로 매우 작습니다
- **비동기**: async/await 패턴으로 메인 스레드 블로킹 없음
- **배터리 영향**: 모바일 기기에서 네트워크 사용으로 인한 배터리 소모는 미미합니다
- **네트워크 부하**: 단일 UDP 패킷 전송이므로 네트워크 부하가 거의 없습니다

## 요구사항

- Unity 2022.1 이상
- .NET Standard 2.1

## 테스트

이 패키지에는 포괄적인 유닛테스트가 포함되어 있습니다.

### 테스트 실행 방법

1. Unity 에디터에서 `Window` → `General` → `Test Runner` 열기
2. `PlayMode` 탭 선택
3. `Run All` 클릭하여 모든 테스트 실행

### 테스트 항목

- ✅ MAC 주소 파싱 (콜론, 하이픈, 구분자 없음)
- ✅ MAC 주소 형식 검증
- ✅ 매직 패킷 구조 검증 (102바이트, 0xFF 헤더, MAC 16회 반복)
- ✅ 브로드캐스트 주소 계산 (다양한 서브넷 마스크)
- ✅ 에러 처리 (잘못된 MAC, 길이 오류 등)

## 라이선스

이 프로젝트의 라이선스는 [LICENSE](LICENSE) 파일을 참조하세요.

## 작성자

- **Seungmin Lee**
- Email: ctrl3d@gmail.com
- Website: https://ctrl3d.work

## 기여

이슈 및 Pull Request는 언제든지 환영합니다!
