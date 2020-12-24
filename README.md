# AutoAttendance

<br>

코로나 바이러스로 인한 비대면 수업으로 학과게시판에 출석 인증용 댓글을 남겨야하는데  
까먹거나 잔다고 시간을 놓쳐서 출석을 못하는 경우가 생겨 이를 방지하기위해 만들었다.

<br>

## 사용 라이브러리
### Nuget Package
- [HtmlAgilityPack](https://html-agility-pack.net)  
- [Newtonsoft.Json](https://www.newtonsoft.com/json)

<br>

## 애플리케이션 정보
- .NET Core 3.1
- Console Application

<br>

## 서버 환경
- .NET Core 3.1.0
- RaspberryPi 3 Model B+
- Raspbian GNU/Linux 10 (buster)

<br>

## 기능
- [x] **show** : 출석체크 대기 목록에있는 아이템들을 출력한다.
- [x] **stop** : 프로그램을 종료한다.

<br>

--------------------------------------------------------

<br>

## 수정

#### 1. DateTime.Parse -> DateTime.ParseExact
    windows 에서는 정상적인 결과를 출력했는데 리눅스 환경에서 실행시키니  
    "System.FormatException: String '12/31' was not recognized as a valid DateTime."  
    라는 DateTime Format 예외가 발생하여 "DateTime.ParseExact" 메서드로 변경해주었다.
