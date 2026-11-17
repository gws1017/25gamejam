using System;
using UnityEngine;

/*****************************************************************************************
 * 파일: AlienBossSettings.cs
 * 역할: 보스2 수치 모음. 파랑/빨강 두 마리가 같은 값을 쓰므로 본체 인스펙터 한 곳에서만 조정한다.
 *       값은 전부 기획서(STAGE2_BOSS_SPEC.md)의 초기 제안값 — 플레이테스트로 조정할 것.
 *****************************************************************************************/

[Serializable]
public class AlienBossSettings
{
    [Header("Common")]
    [Tooltip("폼 이미지 교체 후 첫 공격까지 빈틈(초). 다음 공격을 읽을 시간")]
    [Min(0f)] public float formSwitchPause = 0.6f;
    [Tooltip("순간이동 시 사라지는 시간 = 나타나는 시간(초). 대기 위치 복귀·돌진 줄 이동에 사용")]
    [Min(0f)] public float teleportFadeTime = 0.2f;
    [Tooltip("폼 이미지 4장의 크기가 제각각이라, 화면상 높이를 이 값(월드 유닛)으로 맞춘다")]
    [Min(0.1f)] public float bodyHeight = 1.8f;
    [Tooltip("대기 위치 높이 (화면 비율, 0 = 맨 아래 ~ 1 = 맨 위). 0.68 ≈ 배경 건물 중간쯤 — 상단 UI에 가리지 않게")]
    [Range(0f, 1f)] public float homeViewportY = 0.68f;
    [Tooltip("파랑(본체) 탄의 PoolManager 키")]
    public string blueBulletPoolKey = "AlienBulletBlue";
    [Tooltip("빨강(파트너) 탄의 PoolManager 키")]
    public string redBulletPoolKey = "AlienBulletRed";

    [Header("Form 1 - Basic (aimed single shots)")]
    [Min(1)] public int basicShots = 5;
    [Min(0.05f)] public float basicInterval = 0.6f;

    [Header("Form 2 - Tentacle (warning under feet -> eruption)")]
    [Min(1)] public int tentacleStrikes = 4;
    [Tooltip("전조 장판이 차오르는 시간(초)")]
    [Min(0.05f)] public float tentacleWarning = 1.0f;
    [Tooltip("솟구침 사이 간격(초)")]
    [Min(0f)] public float tentacleInterval = 0.4f;
    [Tooltip("장판 가로 반지름 (월드 유닛). 세로는 GroundSquash 비율로 납작해짐")]
    [Min(0.1f)] public float tentacleRadius = 0.7f;
    [Tooltip("파랑 촉수 이펙트 8프레임 (boss_effect_Tentacle_clean_0~7). 비우면 이펙트 없이 판정만")]
    public Sprite[] tentacleFrames;
    [Tooltip("빨강 촉수 이펙트 8프레임 (boss_effect_TentacleRed_clean_0~7). 비우면 파랑 프레임 사용")]
    public Sprite[] redTentacleFrames;
    [Tooltip("이펙트 시트에서 바닥(물보라)이 가장 넓은 프레임의 폭(px). 장판 크기에 맞춰 이펙트 배율을 계산하는 데 사용 (boss_effect_Tentacle_clean 기준 209)")]
    [Min(1f)] public float tentacleEffectPeakWidthPx = 209f;

    [Header("Form 3 - Dash (lane wave)")]
    [Tooltip("돌진 한 줄의 목표 가로 폭 (월드 유닛). 화면 너비를 빈틈없이 나누도록 실제 폭은 재계산된다")]
    [Min(0.3f)] public float laneWidth = 1.5f;
    [Tooltip("돌진 줄 윗변 높이 (화면 비율). 도로 윗선 — 건물 바로 아래")]
    [Range(0f, 1f)] public float laneTopViewportY = 0.5f;
    [Tooltip("돌진 줄 아랫변 높이 (화면 비율)")]
    [Range(0f, 1f)] public float laneBottomViewportY = 0f;
    [Tooltip("줄 기울기: 윗변이 아랫변보다 오른쪽으로 밀린 거리 (월드 유닛). 음수면 왼쪽, 0이면 세로 줄")]
    public float laneSlant = 1.3f;
    [Tooltip("한 줄의 전조 장판이 차오르는 시간(초)")]
    [Min(0.05f)] public float dashWarning = 1.0f;
    [Tooltip("다음 줄 전조가 시작되기까지의 간격(초). 이전 줄 돌진보다 짧게 잡으면 자동으로 늘어난다")]
    [Min(0.05f)] public float laneInterval = 0.5f;
    [Tooltip("돌진 속도 (월드 유닛/초)")]
    [Min(0.1f)] public float dashSpeed = 16f;
    [Tooltip("옆 줄로 옮길 때 순간이동의 사라짐/나타남 시간(초). 짧을수록 파도가 빨리 넘어간다 (대기 위치 복귀는 teleportFadeTime)")]
    [Min(0f)] public float laneHopFadeTime = 0.05f;

    [Header("Form 4 - Shotgun (3 eyes in sequence, 3 shots)")]
    [Min(1)] public int shotgunVolleys = 3;
    [Tooltip("눈 사이 발사 간격(초) — '타-타-탁' 리듬")]
    [Min(0.05f)] public float eyeDelay = 0.3f;
    [Tooltip("3발 묶음 사이 간격(초)")]
    [Min(0f)] public float volleyInterval = 0.8f;
    [Tooltip("눈 사이 가로 간격 (월드 유닛, bodyHeight 1.8 기준)")]
    public float eyeSpacing = 0.32f;
    [Tooltip("몸 중심에서 눈까지의 높이 (월드 유닛)")]
    public float eyeHeight = 0.55f;
}
