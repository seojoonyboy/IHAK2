# C# 코딩 규칙

1. Line ending : Unix style (/n)
2. Indention : 4 space
3. Block style : 키워드 옆에 붙이는 스타일 , 1줄 블록은 생략가능
   for ( ... ) {
     ...
   }
4. Naming 규칙
    1. 변수명 : camelCasing
        - 지역변수는 약자 정도만 신경써서 간단하게 한다.
    2. 함수명 : PascalCasing (12.10일 기준 정정)
    3. 클래스명 : PascalCasing
        - 클래스의 인스턴스명은 변수명 규칙에 따른다.
    4. 상수 : UPPERCASE_WITH_UNDERSCORE
    5. enum
        1. 정의 : PascalCasing
        2. 내부 프로퍼티: {UPPERCASE_WITH_UNDERSCORE}
        3. enum 변수는 변수명 규칙에 따른다.
5. 주석 규칙
    1. 함수 Summary는 ///를 이용하여 작성.
    2. 사용하지 않는 코드는 왠만하면 제거할것
6. 테스트 코드
    1. Scripting Define Symbols를 이용하여 처리(그냥 작성시 push할때 반드시 주석처리).
    2. 전처리기 명령어 작성시 테스트 코드의 경우 TEST_[Name]으로 작성.