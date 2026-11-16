using System;
using UnityEngine;

// 파랑과 빨강이 함께 사용하는 공격 수치. 본체 인스펙터에서 조정한다.
[Serializable]
public class AlienAttackSettings
{
    // 기본형에서 다음 폼으로 바뀌기 전 발사할 단발 탄환 수
    [Tooltip("기본형에서 다음 폼으로 바뀌기 전 발사할 단발 탄환 수")]
    [Min(1)] public int basicShots = 5;
    // 촉수형에서 발밑 위치를 다시 잡아 전조와 솟구침을 반복할 횟수
    [Tooltip("촉수형에서 발밑 위치를 다시 잡아 전조와 솟구침을 반복할 횟수")]
    [Min(1)] public int tentacleCount = 4;
    // 산탄형의 3발 묶음 반복 횟수. 총 탄환 수는 이 값의 3배
    [Tooltip("산탄형의 3발 묶음 반복 횟수. 총 탄환 수는 이 값의 3배")]
    [Min(1)] public int shotgunBursts = 3;
    // 기본형의 발사 후 대기 시간(초). 산탄형은 3발 묶음 종료 후 추가 대기 시간
    [Tooltip("기본형의 발사 후 대기 시간(초). 산탄형은 3발 묶음 종료 후 추가 대기 시간")]
    [Min(0.05f)] public float shotInterval = 0.65f;
    // 산탄형에서 각 눈의 발사 후 대기 시간(초). 마지막 눈 발사 후에도 적용
    [Tooltip("산탄형에서 각 눈의 발사 후 대기 시간(초). 마지막 눈 발사 후에도 적용")]
    [Min(0.05f)] public float eyeInterval = 0.3f;
    // 폼 이미지 교체 후 첫 공격까지의 빈틈 시간(초)
    [Tooltip("폼 이미지 교체 후 첫 공격까지의 빈틈 시간(초)")]
    [Min(0.05f)] public float formPause = 0.6f;
    // 촉수 솟구침과 돌진 한 줄의 전조가 차오르는 시간(초)
    [Tooltip("촉수 솟구침과 돌진 한 줄의 전조가 차오르는 시간(초)")]
    [Min(0.05f)] public float warningTime = 1f;
    // 촉수 장판의 가로 반지름(월드 단위). 세로 반지름은 이 값의 0.35배
    [Tooltip("촉수 장판의 가로 반지름(월드 단위). 세로 반지름은 이 값의 0.35배")]
    [Min(0.1f)] public float tentacleRadius = 0.8f;
    // 돌진 한 줄의 목표 폭(월드 단위). 화면 너비를 빈틈없이 나누도록 실제 폭은 재계산
    [Tooltip("돌진 한 줄의 목표 폭(월드 단위). 화면 너비를 빈틈없이 나누도록 실제 폭은 재계산")]
    [Min(0.1f)] public float laneWidth = 1.5f;
    // 돌진 중 보스 이동 속도(월드 단위/초)
    [Tooltip("돌진 중 보스 이동 속도(월드 단위/초)")]
    [Min(0.1f)] public float dashSpeed = 16f;
}
