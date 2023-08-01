<h1 align=center>게임 포트 폴리오</h1>
<h3>프로젝트 설명</h3>
보드게임 스페이스 크루를 모바일로 제작했습니다.

---
<h3>게임 플레이 링크</h3>
<div align=center>
  <a href="https://youtu.be/fOlEBwj5XEE">
    <img width=100% height=auto src="https://github.com/ansing8g/Portfolio_SpaceCrew/assets/75000499/9d2647fa-f60e-4215-83c7-418efc6339cb">
  </a>
  이미지를 클릭하면 유투브 링크로 이동합니다.
</div>

---
<h3>서버</h3>
  <li>개발 환경 및 정보</li>
    개발 환경 : .Net 6.0, C Shap, Newtonsoft Json</br>
    경로 : \Server Sources
    </br>
  <li>구현 목표</li>
  LockFree 알고리즘을 최대한 적용한 게임서버 제작을 목표로 했습니다.</br>
  스레드에서 공용으로 사용하는 메모리 최소화하기 위해서 주로 로직처리가 되는 로비와 롬을 스레드위 귀속하는 구조를 선택했습니다.</br>
  여러 스레드가 접근하는 메모리의 경우 thread self 자료구조나 CAS(interlocked)를 사용했습니다.</br>
    </br>
  <li>구조</li>
    <div>
      <img width=100% height=auto src="https://github.com/ansing8g/Portfolio_SpaceCrew/assets/75000499/5529a7a4-df0d-4dd5-ad1e-afcf36afdd2c">
    </div>
    서버에서 사용하는 스레드 그룹은 네트워크 스레드, 워커 스레드, DB 스레드입니다.</br>
    네트워크 스레드에서 패킷을 송신 받으면 소켓객체(유저)에 포함 된 포인터를 통하여 패킷을 전달합니다.</br>
    워커 스레드에는 로비와 롬이 할당되어 전달 받은 패킷을 처리합니다.</br>
    로비와 롬에서 DB처리가 필요 할 때 파일DB 객체에 처리를 요청합니다.</br>
    DB 스레드에서 파일을 읽어서 파일DB 객체에 불러오거나 파일DB 객체 정보를 파일에 저장합니다.</br>
    </br>

---
<h3>클라이언트</h3>
  <li>개발 환경 및 정보</li>
    개발 환경 : Unity 2022.3.23f1</br>
    위치 : \Client APK
    </br>
  <li>기타</li>
    네트워크 라이브러리, 로비, 대기 방, 인게임 UI와 로직 작업을 했습니다.</br>
    그래픽 리소스와 프레임워크를 지인에게 지원 받아 소스는 올리지 않았습니다.</br>
    클라이언트는 \Client APK\SpaceCrew.apk를 설치하여 실행 할 수 있으며 접속 서버는 작성자 개인서버로 타겟되어 있습니다.</br>
    </br>
