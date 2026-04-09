# Team_4DIVE_Quest3 — EDEN

> A VR horror/mystery experience built for Meta Quest 3 using Unity 2022.3 LTS + OpenXR.

---

## 👥 Team Members & Scene Assignments

| 이름 | 학번 | 역할 | 담당 씬 |
|------|------|------|---------|
| 백은혜 (Bai, Xinhui) | 2271002 | 팀장 | MainScene, Scene2_A |
| 김가영 | 2276031 | 팀원 | IntroScene, Scene1_A |
| 정희진 | 2371058 | 팀원 | Scene3_A |
| 신희조 (jo) | 2171025 | 팀원 | Scene4_A |

---

## 🗂️ Scene Overview

| 씬 이름 | 설명 | 담당자 |
|---------|------|--------|
| IntroScene | 게임 인트로 | 김가영 |
| MainScene | 메인 허브 복도 — 각 Set으로 이동하는 문 4개 | 백은혜 |
| Scene1_A | Set 1: 전정(균형감각) 구역 | 김가영 |
| Scene2_A | Set 2: 청각 구역 — 숲 배경, 라디오 퍼즐 | 백은혜 |
| Scene3_A | Set 3: 촉각 구역 | 정희진 |
| Scene4_A | Set 4: 사회적 구역 | 신희조 |

---

## 🛠️ Development Environment

| 항목 | 내용 |
|------|------|
| Unity | 2022.3.62f3 LTS |
| Target Device | Meta Quest 3 |
| OS | Windows 11 |
| XR Framework | OpenXR (`com.unity.xr.openxr`) |
| Meta SDK | Meta XR All-in-One SDK v85.0.0 |
| Input System | Unity Input System 1.7.0 + `UnityEngine.XR.InputDevices` |
| XR Management | com.unity.xr.management 4.5.4 |
| Version Control | Git / GitHub |

---

## 📁 Project Structure

```
Assets/
└── HW_09/
    ├── Scenes/
    │   ├── MainScene.unity
    │   ├── IntroScene.unity
    │   ├── Scene1_A.unity / Scene1_B.unity
    │   ├── Scene2_A.unity / Scene2_B.unity
    │   ├── Scene3_A.unity / Scene3_B.unity
    │   ├── Scene4_A.unity / Scene4_B.unity
    │   └── OutroScene.unity
    ├── Scripts/
    └── ...
```

---

## ▶️ How to Build & Run

1. Unity 2022.3.62f3 LTS 에서 프로젝트 열기
2. **File > Build Settings** → Platform: **Android** 선택
3. **XR Plug-in Management** → **OpenXR** 체크 확인
4. `RegisterScenes` MenuItem 실행하여 씬 9개 일괄 등록 확인
5. Meta Quest 3 기기 연결 후 **Build and Run**

> ⚠️ `com.unity.xr.oculus`와 `com.unity.xr.openxr` 동시 설치 금지 — OpenXR 단독 사용

---

## ⚠️ Known Issues / Notes

- `PlayerController.cs`의 `cameraTransform`은 반드시 Inspector에서 **Main Camera** 오브젝트를 수동 연결
- 모든 스크립트는 `namespace HW09 { }` 로 감싸야 클래스명 충돌 방지
- 씬 파일은 각자 담당 폴더에서 관리, **MainScene은 팀장(백은혜)이 단독 관리**
- Door_01~04의 `triggerRadius`는 **3.0** 으로 설정되어 있음 (VR 환경 대응)

---

## 🎙️ Assets & Voice

| 항목 | 출처 |
|------|------|
| 메인 허브 배경 | SciFi Warehouse Kit |
| 숲 배경 (Scene2) | Fantasy Forest Environment Free Sample |
| AI 음성 (System) | 클로바더빙 — 오렌지호수 보이스 |
