# 🗨️ C#과 Redis를 활용한 실시간 멀티 채팅 서버, 클라이언트

C# 비동기 소켓 프로그래밍과 Redis를 사용하여 구현한 실시간 멀티 클라이언트 채팅 서버 및 클라이언트 콘솔 프로그램입니다.

---

## 🔧 기술 스택

- **언어**: C#
- **프레임워크**: .NET (콘솔 애플리케이션)
- **네트워킹**: TCP 소켓 (BeginReceive, BeginSend 기반 비동기 처리)
- **데이터베이스**: Redis
---

## ✅ 주요 기능

- ✔️ 여러 클라이언트 간 실시간 메시지 전송
- ✔️ Redis를 이용한 채팅 기록 저장
- ✔️ 비동기 TCP 소켓 기반 다중 클라이언트 처리
- ✔️ 클라이언트 강제 종료 감지 및 처리
- ✔️ 간단한 콘솔 기반 채팅 인터페이스

---

## 📁 프로젝트 구조
ChatServer/
├── Server.cs # 클라이언트 연결 및 Redis 처리

ChatClient/
├── Client.cs # 사용자 입력 처리 및 메시지 출력

---

## ⚙️ 작동 방식
### 서버
1. 클라이언트의 TCP 연결을 비동기적으로 수신합니다.
2. 클라이언트가 채팅 수신자들 이름과 메시지를 입력하면 Redis에 수신자들을 그룹화하여 채팅 메시지를 기록합니다.
3. 수신된 메시지를 그룹화된 클라이언트들에게 전송합니다.
4. 클라이언트 연결이 끊어져도 Redis에서 로그를 가져와 클라이언트에게 전송합니다.

### 클라이언트
1. 서버에 TCP로 연결합니다.
2. 사용자가 수신자와 메시지를 입력하면 서버에 전송합니다.
3. 서버로부터 수신된 메시지를 실시간으로 표시합니다.
![새로운-프로젝트](https://github.com/user-attachments/assets/b3171e82-9fea-427b-85a5-b17747690582)
