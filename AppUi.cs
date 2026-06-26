using System.Globalization;
using System.Text.RegularExpressions;

namespace Memory_Storage;

public enum AppLanguage
{
    SimplifiedChinese,
    TraditionalChinese,
    Japanese,
    Korean,
    German,
    French,
    Italian,
    AmericanEnglish,
    BritishEnglish
}

public static class AppUi
{
    private const string DarkModePreferenceKey = "memory_storage_dark_mode";

    private static readonly Dictionary<string, string[]> Texts = new()
    {
        ["AppTitle"] = ["记忆储仓", "記憶儲倉", "メモリーストレージ", "메모리 저장소", "Speicherlager", "Stockage Memoire", "Archivio Memoria", "Memory Storage", "Memory Storage"],
        ["Back"] = ["返回", "返回", "戻る", "뒤로", "Zuruck", "Retour", "Indietro", "Back", "Back"],
        ["Functions"] = ["功能", "功能", "機能", "기능", "Funktionen", "Fonctions", "Funzioni", "Functions", "Functions"],
        ["SidePanel"] = ["侧边栏", "側邊欄", "サイドパネル", "사이드 패널", "Seitenleiste", "Panneau lateral", "Pannello laterale", "Side panel", "Side panel"],
        ["RecordOpenedPrograms"] = ["记录打开的程序", "記錄開啟的程式", "開いたプログラムを記録", "열었던 프로그램 기록", "Geoffnete Programme", "Programmes ouverts", "Programmi aperti", "Record opened programs", "Record opened programmes"],
        ["RecordActivityDuration"] = ["记录活动时长", "記錄活動時長", "利用時間を記録", "활動 시간 기록", "Aktivitatsdauer", "Duree d'activite", "Durata attivita", "Record activity duration", "Record activity running time"],
        ["TimelineView"] = ["时间轴检视", "時間軸檢視", "タイムライン表示", "타임라인 보기", "Zeitachse", "Chronologie", "Vista sequenza", "Timeline view", "Timeline review"],
        ["SearchFunction"] = ["搜索功能记录", "搜尋功能記錄", "検索機能記録", "검색 기능 기록", "Suchprotokoll", "Journal de recherche", "Registro ricerca", "Search function records", "Search function records log"],
        ["Login"] = ["登录", "登入", "ログイン", "로그인", "Anmelden", "Connexion", "Accesso", "Login", "Sign in"],
        ["Register"] = ["注册", "註冊", "登録", "가입", "Registrieren", "Inscription", "Registrati", "Register", "Register"],
        ["SearchPlaceholder"] = ["搜索应用、项目、文件、备注或柜号", "搜尋應用程式、專案、檔案、備註或櫃號", "アプリ、プロジェクト、ファイル、メモ、番号を検索", "앱, 프로젝트, 파일, 메모, 번호 검색", "App, Projekt, Datei, Notiz oder Nummer suchen", "Rechercher app, projet, fichier, note ou code", "Cerca app, progetto, file, nota o codice", "Search app, project, file, note, or cabinet code", "Search for an app, project, file, note or cabinet code"],
        ["NoRecords"] = ["还没有记录", "尚無紀錄", "記録はまだありません", "아직 기록 없음", "Noch keine Eintrage", "Aucun enregistrement", "Nessun record", "No records yet", "No records as yet"],
        ["FreshSession"] = ["关闭应用后，本次记录会自动清空。", "關閉 App 後，本次紀錄會自動清空。", "アプリを閉じると今回の記録は消えます。", "앱을 닫으면 이번 기록은 지워집니다.", "Diese Sitzung wird beim Schliessen geleert.", "Cette session est effacee a la fermeture.", "La sessione viene cancellata alla chiusura.", "This session starts fresh and clears automatically when the app closes.", "This session starts afresh and is cleared automatically when the app closes."],
        ["MemoryRecordFile"] = ["记忆记录文件", "記憶紀錄文件", "メモリ記録ファイル", "메모리 기록 파일", "Memory Record Datei", "Fichier Memory Record", "File Memory Record", "Memory Record File", "Memory Record File"],
        ["Today"] = ["今天", "今天", "今日", "오늘", "Heute", "Aujourd'hui", "Oggi", "Today", "Today"],
        ["Settings"] = ["设置", "設定", "設定", "설정", "Einstellungen", "Parametres", "Impostazioni", "Settings", "Settings"],
        ["Language"] = ["语言", "語言", "言語", "언어", "Sprache", "Langue", "Lingua", "Language", "Language"],
        ["ChooseLanguage"] = ["选择语言", "選擇語言", "言語を選択", "언어 선택", "Sprache wahlen", "Choisir la langue", "Scegli lingua", "Choose language", "Choose language"],
        ["DarkMode"] = ["夜间模式", "夜間模式", "ダークモード", "다크 모드", "Dunkelmodus", "Mode sombre", "Modalita scura", "Dark mode", "Dark mode"],
        ["LightMode"] = ["日间模式", "日間模式", "ライトモード", "라이트 모드", "Hellmodus", "Mode clair", "Modalita chiara", "Light mode", "Light mode"],
        ["LoginProfile"] = ["登录个人资料", "登入個人資料", "プロフィールログイン", "프로필 로그인", "Profil anmelden", "Connexion profil", "Accesso profilo", "Login Profile", "Login Profile"],
        ["RegisterProfile"] = ["注册个人资料", "註冊個人資料", "プロフィール登録", "프로필 가입", "Profil registrieren", "Inscription profil", "Registrazione profilo", "Register Profile", "Register Profile"],
        ["Email"] = ["电子邮件", "電子郵件", "メール", "이메일", "E-Mail", "E-mail", "Email", "Email", "Email"],
        ["Password"] = ["密码", "密碼", "パスワード", "비밀번호", "Passwort", "Mot de passe", "Password", "Password", "Password"],
        ["FullName"] = ["姓名", "姓名", "氏名", "이름", "Vollstandiger Name", "Nom complet", "Nome completo", "Full name", "Name in full"],
        ["Show"] = ["显示", "顯示", "表示", "보기", "Zeigen", "Afficher", "Mostra", "Show", "Show"],
        ["Hide"] = ["隐藏", "隱藏", "非表示", "숨기기", "Verbergen", "Masquer", "Nascondi", "Hide", "Hide"],
        ["Confirm"] = ["确定", "確定", "確認", "확인", "Bestatigen", "Confirmer", "Conferma", "Confirm", "Confirm and continue"],
        ["Tagline"] = ["整理今天开启的应用、项目、文件与学习活动。", "整理今天開啟的應用程式、專案、檔案與學習活動。", "今日開いたアプリ、プロジェクト、ファイル、学習活動を整理します。", "오늘 연 앱, 프로젝트, 파일, 학습 활동을 정리합니다.", "Organisiert heutige Apps, Projekte, Dateien und Lernaktivitat.", "Organise les apps, projets, fichiers et etudes du jour.", "Organizza app, progetti, file e studio di oggi.", "Organize every app, project, file, and study activity you open today.", "Organise every app, project, file, and study activity you open today."],
        ["Start"] = ["开始", "開始", "開始", "시작", "Start", "Demarrer", "Avvia", "Start", "Start"],
        ["AddRecord"] = ["新增记录", "新增紀錄", "記録を追加", "기록 추가", "Eintrag hinzufugen", "Ajouter un enregistrement", "Aggiungi record", "Add Record", "Add a Record"],
        ["Time"] = ["时间", "時間", "時間", "시간", "Zeit", "Heure", "Ora", "Time", "Time"],
        ["AppName"] = ["应用名称", "應用程式名稱", "アプリ名", "앱 이름", "App-Name", "Nom de l'app", "Nome app", "App name", "Application name"],
        ["ProjectName"] = ["项目名称", "專案名稱", "プロジェクト名", "프로젝트 이름", "Projektname", "Nom du projet", "Nome progetto", "Project name", "Project name"],
        ["FileName"] = ["文件名称", "檔案名稱", "ファイル名", "파일 이름", "Dateiname", "Nom du fichier", "Nome file", "File name", "File name"],
        ["Note"] = ["备注", "備註", "メモ", "메모", "Notiz", "Note", "Nota", "Note", "Note"],
        ["Save"] = ["保存", "儲存", "保存", "저장", "Speichern", "Enregistrer", "Salva", "Save", "Save"],
        ["OpenedEmpty"] = ["目前尚未记录开启的程序。", "目前尚未記錄開啟的程式。", "開いたプログラムはまだありません。", "열린 프로그램 기록이 없습니다.", "Noch keine Programme erfasst.", "Aucun programme ouvert.", "Nessun programma registrato.", "No opened programs have been recorded in this session yet.", "No opened programmes have been recorded during this session yet."],
        ["DurationEmpty"] = ["目前尚未记录活动时长。", "目前尚未記錄活動時長。", "利用時間はまだ記録されていません。", "활동 시간이 아직 기록되지 않았습니다.", "Noch keine Dauer erfasst.", "Aucune duree enregistree.", "Nessuna durata registrata.", "No activity duration has been recorded in this session yet.", "No activity running time has been recorded during this session yet."],
        ["TimelineEmpty"] = ["还没有时间轴记录。", "尚無時間軸紀錄。", "タイムライン記録はまだありません。", "타임라인 기록이 없습니다.", "Noch keine Zeitachse.", "Aucune chronologie.", "Nessuna sequenza.", "No timeline records yet.", "No timeline records as yet."],
        ["SearchFirst"] = ["搜索：请先输入关键字。\n\n找到：\n还没有搜索结果。", "搜尋：請先輸入關鍵字。\n\n找到：\n尚無搜尋結果。", "検索：先にキーワードを入力してください。\n\n結果：\nまだありません。", "검색: 먼저 키워드를 입력하세요.\n\n찾음:\n검색 결과 없음.", "Suche: Bitte zuerst ein Stichwort eingeben.\n\nGefunden:\nKeine Ergebnisse.", "Recherche : saisissez d'abord un mot-cle.\n\nTrouve :\nAucun resultat.", "Ricerca: inserisci prima una parola chiave.\n\nTrovato:\nNessun risultato.", "Search: Type a keyword first.\n\nFound:\nNo search results yet.", "Search: Enter a keyword first.\n\nFound:\nNo search results as yet."],
        ["SearchLabel"] = ["搜索", "搜尋", "検索", "검색", "Suche", "Recherche", "Ricerca", "Search", "Search"],
        ["FoundLabel"] = ["找到", "找到", "結果", "찾음", "Gefunden", "Trouve", "Trovato", "Found", "Found"],
        ["NoSearchResults"] = ["没有符合的记忆记录文件。", "沒有符合的記憶紀錄文件。", "一致する記録ファイルはありません。", "일치하는 기록 파일이 없습니다.", "Keine passenden Dateien.", "Aucun fichier correspondant.", "Nessun file corrispondente.", "No matching memory record files.", "No matching memory record files were found."],
        ["Opened"] = ["打开", "開啟", "開いた", "열림", "Geoffnet", "Ouvert", "Aperto", "Opened", "Opened"],
        ["LessThanOneMinute"] = ["少于 1 分钟", "少於 1 分鐘", "1分未満", "1분 미만", "Weniger als 1 Minute", "Moins d'une minute", "Meno di 1 minuto", "Less than 1 minute", "Less than 1 minute"],
        ["Minutes"] = ["分钟", "分鐘", "分", "분", "Minuten", "minutes", "minuti", "minutes", "minutes"],
        ["Hour"] = ["小时", "小時", "時間", "시간", "Stunde", "heure", "ora", "hour", "hour"],
        ["Hours"] = ["小时", "小時", "時間", "시간", "Stunden", "heures", "ore", "hours", "hours"],
        ["Now"] = ["现在", "現在", "現在", "현재", "Jetzt", "Maintenant", "Ora", "Now", "Now"],
        ["Project"] = ["项目", "專案", "プロジェクト", "프로젝트", "Projekt", "Projet", "Progetto", "Project", "Project"],
        ["File"] = ["文件", "檔案", "ファイル", "파일", "Datei", "Fichier", "File", "File", "File"],
        ["NoProjectOrFile"] = ["没有项目或文件", "沒有專案或檔案", "プロジェクトまたはファイルなし", "프로젝트 또는 파일 없음", "Kein Projekt oder Datei", "Aucun projet ou fichier", "Nessun progetto o file", "No project or file attached", "No project or file attached"],
        ["ConfirmPassword"] = ["确认密码", "再次輸入密碼", "パスワードを再入力", "비밀번호 다시 입력", "Passwort bestatigen", "Confirmer le mot de passe", "Conferma password", "Confirm password", "Confirm password"],
        ["ValidationName"] = ["姓名至少需要 2 个字。", "姓名至少需要 2 個字。", "名前は2文字以上で入力してください。", "이름은 2자 이상이어야 합니다.", "Der Name muss mindestens 2 Zeichen enthalten.", "Le nom doit contenir au moins 2 caracteres.", "Il nome deve contenere almeno 2 caratteri.", "Name must contain at least 2 characters.", "Name must contain at least 2 characters."],
        ["ValidationEmail"] = ["请输入正确的电子邮件格式。", "請輸入正確的電子郵件格式。", "有効なメールアドレスを入力してください。", "올바른 이메일 형식을 입력하세요.", "Bitte geben Sie eine gultige E-Mail-Adresse ein.", "Veuillez saisir une adresse e-mail valide.", "Inserisci un indirizzo email valido.", "Please enter a valid email address.", "Please enter a valid email address."],
        ["ValidationPassword"] = ["密码至少 8 个字，并且必须包含英文字母和数字。", "密碼至少 8 個字，並且必須包含英文字母和數字。", "パスワードは8文字以上で、英字と数字を含めてください。", "비밀번호는 8자 이상이며 문자와 숫자를 포함해야 합니다.", "Das Passwort muss mindestens 8 Zeichen lang sein und Buchstaben sowie Zahlen enthalten.", "Le mot de passe doit contenir au moins 8 caracteres avec des lettres et des chiffres.", "La password deve avere almeno 8 caratteri e includere lettere e numeri.", "Password must be at least 8 characters and include letters and numbers.", "Password must be at least 8 characters and include letters and numbers."],
        ["ValidationPasswordMatch"] = ["两次输入的密码必须相同。", "兩次輸入的密碼必須相同。", "2つのパスワードは一致する必要があります。", "두 비밀번호가 일치해야 합니다.", "Beide Passwortfelder mussen ubereinstimmen.", "Les deux mots de passe doivent correspondre.", "Le due password devono corrispondere.", "The two password fields must match.", "The two password fields must match."],
        ["ValidationLoginMismatch"] = ["电子邮件或密码与注册资料不一致。", "電子郵件或密碼與註冊資料不一致。", "メールまたはパスワードが登録情報と一致しません。", "이메일 또는 비밀번호가 등록 정보와 일치하지 않습니다.", "E-Mail oder Passwort stimmt nicht mit dem registrierten Profil uberein.", "L'e-mail ou le mot de passe ne correspond pas au profil inscrit.", "Email o password non corrispondono al profilo registrato.", "Email or password does not match the registered profile.", "Email or password does not match the registered profile."],
        ["RegistrationCompleteTitle"] = ["注册完成", "註冊完成", "登録完了", "등록 완료", "Registrierung abgeschlossen", "Inscription terminee", "Registrazione completata", "Registration complete", "Registration complete"],
        ["RegistrationCompleteBody"] = ["你的个人资料已储存在本次 App 使用期间。", "你的個人資料已儲存在本次 App 使用期間。", "プロフィール情報は今回のアプリ使用中に保存されました。", "프로필 정보가 현재 앱 세션에 저장되었습니다.", "Ihre Profildaten wurden fur diese App-Sitzung gespeichert.", "Vos donnees de profil sont enregistrees pour cette session.", "I dati del profilo sono stati salvati per questa session.", "Your profile data has been saved for this app session.", "Your profile data has been saved for this app session."],
        ["LoginCompleteTitle"] = ["登入完成", "登入完成", "ログイン完了", "로그인 완료", "Anmeldung abgeschlossen", "Connexion terminee", "Accesso completato", "Login complete", "Login complete"],
        ["LoginCompleteBody"] = ["你已成功登入个人资料。", "你已成功登入個人資料。", "プロフィールにログインしました。", "프로필에 로그인했습니다.", "Sie sind in Ihrem Profil angemeldet.", "Votre profil est connecte.", "Hai effettuato l'accesso al profilo.", "Your profile is logged in.", "Your profile is logged in."],
        ["ChooseAvatarImage"] = ["选择头像图片", "選擇頭像圖片", "アバター画像を選択", "프로필 이미지 선택", "Avatarbild auswahlen", "Choisir une image de profil", "Scegli immagine profilo", "Choose avatar image", "Choose avatar image"],
        ["Profile"] = ["个人档案", "個人檔案", "プロフィール", "개인 프로필", "Profil", "Profil", "Profilo", "Profile", "Profile"],
        ["UserName"] = ["使用者姓名", "使用者姓名", "ユーザー名", "사용자 이름", "Benutzername", "Nom d'utilisateur", "Nome utente", "User name", "User name"],
        ["Logout"] = ["登出账号", "登出帳號", "ログアウト", "로그아웃", "Abmelden", "Se deconnecter", "Esci", "Log out", "Sign out"],
        ["ChangeAvatar"] = ["更换头像", "更換頭像", "アバターを変更", "프로필 이미지 변경", "Avatar andern", "Changer l'avatar", "Cambia avatar", "Change avatar", "Change profile picture"],
        ["NotLoggedIn"] = ["尚未登入", "尚未登入", "ログインしていません", "로그인하지 않았습니다", "Nicht angemeldet", "Non connecte", "Accesso non effettuato", "Not logged in", "Not signed in"],
        ["AvatarLoginRequired"] = ["请先注册后再登入帐号密码，这样才能使用头像", "請先註冊後再登入帳密，這樣才能使用頭像", "アバターを使用するには、先に登録してからログインしてください。", "아바타를 사용하려면 먼저 가입한 뒤 계정과 비밀번호로 로그인하세요.", "Bitte registrieren Sie sich zuerst und melden Sie sich danach an, um den Avatar zu verwenden.", "Veuillez d'abord vous inscrire puis vous connecter pour utiliser l'avatar.", "Registrati prima e poi accedi per usare l'avatar.", "Please register first, then log in to use the avatar.", "Please create an account first, then sign in to use the profile picture."],
        ["Warning"] = ["警告", "警告", "警告", "경고", "Warnung", "Avertissement", "Avviso", "Warning", "Warning"],
        ["Done"] = ["完成", "完成", "完了", "완료", "Fertig", "Termine", "Fine", "Done", "All done"],
        ["MemberRegisterSuccess"] = ["注册成功！", "註冊成功！", "登録成功！", "가입 성공!", "Registrierung erfolgreich!", "Inscription reussie !", "Registrazione riuscita!", "Registration successful!", "Registration completed successfully!"],
        ["MemberLoginSuccess"] = ["登入成功！", "登入成功！", "ログイン成功！", "로그인 성공!", "Anmeldung erfolgreich!", "Connexion reussie !", "Accesso riuscito!", "Login successful!", "Sign-in successful!"],
        ["HomeEyebrow"] = ["今天的记忆工作区", "今天的記憶工作區", "今日の記憶ワークスペース", "오늘의 기억 작업 공간", "Heutiger Speicherbereich", "Espace memoire du jour", "Area memoria di oggi", "Today memory workspace", "Today's memory workspace"],
        ["AppUserGuide"] = ["应用程式使用指南", "App 使用指南", "アプリ利用ガイド", "앱 사용자 가이드", "App-Benutzerhandbuch", "Guide d'utilisation", "Guida utente app", "App User Guide", "App User Guide"],
        ["PrivacyStatement"] = ["隐私声明", "隱私聲明", "プライバシー声明", "개인정보 보호 성명", "Datenschutzerklarung", "Declaration de confidentialite", "Informativa sulla privacy", "Privacy Statement", "Privacy Notice"],
        ["AppUserGuideBody"] = ["1. 只会记录使用者在电脑桌面任何应用程式中用滑鼠游标选取的项目。\n\n2. 未登入帐户时：关闭并重新启动应用程式会自动清除所有记录，所有资料都会永久删除。\n\n3. 已登入帐户时：关闭并重新启动应用程式会还原目前的文件柜图示数量，以及每个图示内的所有记录。\n\n4. 使用者在本应用程式中的操作只有：注册与登入、设定个人资料、点击左侧栏功能来查看记录并决定继续或停止记录、设定按钮，以及在搜寻栏中搜寻相关保存资料。", "1. 只會記錄使用者在電腦桌面任何應用程式中用滑鼠游標選取的項目。\n\n2. 未登入帳戶時：關閉並重新啟動應用程式會自動清除所有紀錄，所有資料都會永久刪除。\n\n3. 已登入帳戶時：關閉並重新啟動應用程式會還原目前的文件檔案櫃圖示數量，以及每個圖示內的所有紀錄。\n\n4. 使用者在本應用程式中的操作只有：註冊與登入、設定個人資料、點擊左側欄功能來查看紀錄並決定繼續或停止記錄、設定按鈕，以及在搜尋欄中搜尋相關保存資料。", "1. パソコンのデスクトップ上にある任意のアプリで、ユーザーがマウスカーソルで選択した項目のみを記録します。\n\n2. アカウントにログインしていない場合：アプリを閉じて再起動すると、すべての記録が自動的に消去され、データは完全に削除されます。\n\n3. アカウントにログインしている場合：アプリを閉じて再起動すると、現在のファイルキャビネットアイコン数と各アイコン内のすべての記録が復元されます。\n\n4. このアプリでユーザーが行う操作は、登録とログイン、個人プロフィールの設定、左サイドバーの機能をクリックして記録を確認し記録を続けるか停止するかを決めること、設定ボタンの調整、検索バーで保存済みデータを検索することです。", "1. 컴퓨터 데스크톱의 모든 애플리케이션에서 사용자가 마우스 커서로 선택한 항목만 기록합니다.\n\n2. 계정에 로그인하지 않은 경우: 애플리케이션을 닫고 다시 시작하면 모든 기록이 자동으로 지워지며 모든 데이터가 영구적으로 삭제됩니다.\n\n3. 계정에 로그인한 경우: 애플리케이션을 닫고 다시 시작하면 현재 파일 캐비닛 아이콘 수와 각 아이콘 안의 모든 기록이 복원됩니다.\n\n4. 이 애플리케이션에서 사용자가 할 수 있는 작업은 등록 및 로그인, 개인 프로필 설정, 왼쪽 사이드바 기능을 클릭하여 기록을 보고 기록을 계속할지 중지할지 결정하기, 설정 버튼 구성, 검색창에서 저장된 관련 데이터 검색입니다.", "1. Es werden nur Elemente aufgezeichnet, die der Benutzer mit dem Mauszeiger in einer Desktop-Anwendung auswählt.\n\n2. Ohne angemeldetes Konto: Beim Schliessen und Neustarten der Anwendung werden alle Aufzeichnungen automatisch dauerhaft gelöscht.\n\n3. Mit angemeldetem Konto: Beim Schliessen und Neustarten der Anwendung werden die aktuelle Anzahl der Aktenschrank-Symbole und alle darin enthaltenen Aufzeichnungen wiederhergestellt.\n\n4. Die einzigen Aktionen des Benutzers in dieser Anwendung sind: Registrieren und Anmelden, Profile einrichten, Funktionen in der linken Seitenleiste anklicken, um Aufzeichnungen anzuzeigen und zu entscheiden, ob die Aufzeichnung fortgesetzt oder gestoppt wird, Einstellungen konfigurieren und gespeicherte Daten über die Suchleiste suchen.", "1. Enregistre uniquement les elements selectionnes par le curseur de la souris dans toute application du bureau.\n\n2. Sans compte connecte : fermer puis redemarrer l'application efface automatiquement tous les enregistrements de facon permanente.\n\n3. Avec un compte connecte : fermer puis redemarrer l'application restaure le nombre actuel d'icones de classeurs et tous les enregistrements contenus dans ces icones.\n\n4. Les seules actions de l'utilisateur dans cette application sont : s'inscrire et se connecter, configurer le profil personnel, cliquer sur les fonctions de la barre laterale gauche pour consulter les enregistrements et decider de continuer ou d'arreter l'enregistrement, configurer les boutons de parametres et rechercher des donnees enregistrees dans la barre de recherche.", "1. Registra solo gli elementi selezionati dal cursore del mouse dell'utente in qualsiasi applicazione sul desktop del computer.\n\n2. Senza account connesso: chiudere e riavviare l'applicazione cancellera automaticamente tutti i record, eliminando definitivamente tutti i dati.\n\n3. Con account connesso: chiudere e riavviare l'applicazione ripristinera il numero attuale di icone degli schedari e tutti i record contenuti in esse.\n\n4. Le uniche azioni dell'utente in questa applicazione sono: registrarsi e accedere, configurare i profili personali, fare clic sulle funzioni nella barra laterale sinistra per visualizzare i record e decidere se continuare o interrompere la registrazione, configurare i pulsanti delle impostazioni e cercare dati salvati nella barra di ricerca.", "1. Only records items selected by the user's mouse cursor on any application on the computer desktop.\n\n2. Without an account logged in: Closing and restarting the application will automatically clear all records, permanently erasing all data.\n\n3. With an account logged in: Closing and restarting the application will restore the current number of file cabinet icons and all records within those icons.\n\n4. The user's only actions in this application are: registering and logging in, setting up personal profiles, clicking on functions in the left sidebar to view records and decide whether to continue or stop recording, configuring the settings buttons, and searching for relevant saved data in the search bar.", "1. Records only the items selected with the user's mouse pointer in any desktop application on the computer.\n\n2. Without an account signed in: closing and restarting the application will automatically clear all records, permanently removing all data.\n\n3. With an account signed in: closing and restarting the application will restore the current number of file-cabinet icons and all records within those icons.\n\n4. The user's actions in this application are limited to: registering and signing in, setting up personal profiles, clicking functions in the left-hand sidebar to review records and decide whether to continue or stop recording, configuring the settings controls, and searching for relevant saved data in the search bar."],
        ["PrivacyStatementBody"] = ["关于使用者隐私，开发者不会在后台直接操作本应用程式，也不会在后台记录资料。本应用程式只会表面记录电脑应用程式资料。也就是说，成功登入并在自己电脑桌面应用程式中互动的使用者，其资料会被记录，但只有使用者本人可以看见。开发者不会看到其他使用者的隐私。请放心使用这款「Memory Storage」应用程式。", "關於使用者隱私，開發者不會在背景直接操作本應用程式，也不會在背景記錄資料。本應用程式只會表面記錄電腦應用程式資料。也就是說，成功登入並在自己電腦桌面應用程式中互動的使用者，其資料會被記錄，但只有使用者本人可以看見。開發者不會看到其他使用者的隱私。請放心使用這款「Memory Storage」App。", "ユーザーのプライバシーについて、開発者がバックグラウンドでこのアプリを直接操作することはなく、バックグラウンドでデータを記録することもありません。このアプリは、コンピューター上のアプリ操作データを表面的に記録するだけです。つまり、ログインに成功したユーザーが自分のデスクトップアプリで操作したデータは記録されますが、それを見ることができるのはユーザー本人だけです。開発者が他のユーザーのプライバシーを見ることはありません。安心して「Memory Storage」アプリをご利用ください。", "사용자 개인정보와 관련하여 개발자는 백그라운드에서 이 애플리케이션을 직접 조작하지 않으며 백그라운드에서 데이터를 기록하지 않습니다. 이 애플리케이션은 컴퓨터 애플리케이션 데이터를 표면적으로만 기록합니다. 즉, 성공적으로 로그인한 사용자가 자신의 컴퓨터 데스크톱 애플리케이션에서 상호작용한 데이터는 기록되지만, 그 내용은 사용자 본인만 볼 수 있습니다. 개발자는 다른 사용자의 개인정보를 볼 수 없습니다. 안심하고 \"Memory Storage\" 앱을 사용하세요.", "Zum Datenschutz der Benutzer: Der Entwickler bedient diese Anwendung nicht direkt im Hintergrund und zeichnet dort auch keine Daten auf. Diese Anwendung erfasst Computerdaten nur oberflachlich. Das bedeutet: Daten eines erfolgreich angemeldeten Benutzers, der mit Desktop-Anwendungen interagiert, werden aufgezeichnet, sind aber nur fur den Benutzer selbst sichtbar. Der Entwickler sieht die Privatsphare anderer Benutzer nicht. Bitte verwenden Sie die App \"Memory Storage\" unbesorgt.", "Concernant la confidentialite des utilisateurs, le developpeur n'operera pas directement cette application en arriere-plan et n'y enregistrera pas de donnees. Cette application enregistre seulement de facon superficielle les donnees des applications de l'ordinateur. Les donnees d'un utilisateur connecte qui interagit avec les applications de bureau de son ordinateur seront enregistrees, mais seul l'utilisateur pourra les voir. Le developpeur ne verra pas la confidentialite des autres utilisateurs. Vous pouvez utiliser l'application \"Memory Storage\" en toute confiance.", "Per quanto riguarda la privacy dell'utente, lo sviluppatore non gestira direttamente questa applicazione in background e non registrera dati in background. Questa applicazione registra solo superficialmente i dati delle applicazioni del computer. Cio significa che i dati di un utente connesso che interagisce con le applicazioni desktop del proprio computer verranno registrati, ma solo l'utente stesso potra vederli. Lo sviluppatore non vedra la privacy degli altri utenti. Usa tranquillamente l'app \"Memory Storage\".", "Regarding user privacy, the developer will not directly operate this application in the background, nor will it record data in the background. This application will only superficially record computer application data. That is, the data of a user who has successfully logged in and interacts with the desktop application on their computer will be recorded, but only the user themselves can see it. The developer will not see the privacy of other users. Please feel free to use this \"Memory Storage\" app.", "Regarding user privacy, the developer will not directly operate this application in the background, nor will it record data in the background. This application records computer application data only at a surface level. In other words, when a signed-in user interacts with desktop applications on their computer, those records are saved for that user only. The developer cannot view the private records of other users. Please feel free to use the \"Memory Storage\" app."]
        ,["ClearSelectedItems"] = ["清除选取项目", "清除勾選項目", "選択した項目を削除", "선택 항목 지우기", "Ausgewahlte Elemente loschen", "Effacer les elements selectionnes", "Cancella elementi selezionati", "Clear selected items", "Clear ticked items"],
        ["ClearAllItems"] = ["清除全部项目", "清除全部項目", "すべての項目を削除", "모든 항목 지우기", "Alle Elemente loschen", "Effacer tous les elements", "Cancella tutti gli elementi", "Clear all items", "Clear every item"],
        ["Trash"] = ["垃圾桶", "垃圾桶", "ゴミ箱", "휴지통", "Papierkorb", "Corbeille", "Cestino", "Trash", "Bin"],
        ["DeleteMode"] = ["删除模式", "刪除模式", "削除モード", "삭제 모드", "Loschmodus", "Mode suppression", "Modalita eliminazione", "Delete mode", "Removal mode"],
        ["NoSelectedItems"] = ["请先勾选要清除的文件柜。", "請先勾選要清除的文件檔案櫃。", "削除するファイルキャビネットを選択してください。", "삭제할 파일 캐비닛을 먼저 선택하세요.", "Bitte wahlen Sie zuerst die zu loschenden Aktenschranke aus.", "Veuillez d'abord selectionner les classeurs a effacer.", "Seleziona prima gli schedari da cancellare.", "Please select the file cabinets you want to clear first.", "Please tick the file cabinets you want to clear first."],
        ["ConfirmClearSelected"] = ["确定要清除已勾选的文件柜与内部记录吗？", "確定要清除已勾選的文件檔案櫃與內部紀錄嗎？", "選択したファイルキャビネットと内部記録を削除しますか？", "선택한 파일 캐비닛과 내부 기록을 지울까요?", "Ausgewahlte Aktenschranke und enthaltene Aufzeichnungen loschen?", "Effacer les classeurs selectionnes et leurs enregistrements ?", "Cancellare gli schedari selezionati e i record interni?", "Clear the selected file cabinets and their internal records?", "Clear the ticked file cabinets and their internal records?"],
        ["ConfirmClearAll"] = ["确定要清除全部文件柜与内部记录吗？", "確定要清除全部文件檔案櫃與內部紀錄嗎？", "すべてのファイルキャビネットと内部記録を削除しますか？", "모든 파일 캐비닛과 내부 기록을 지울까요?", "Alle Aktenschranke und enthaltenen Aufzeichnungen loschen?", "Effacer tous les classeurs et leurs enregistrements ?", "Cancellare tutti gli schedari e i record interni?", "Clear all file cabinets and their internal records?", "Clear every file cabinet and all records inside them?"],
        ["Cancel"] = ["取消", "取消", "キャンセル", "취소", "Abbrechen", "Annuler", "Annulla", "Cancel", "Cancel action"],
        ["Clear"] = ["清除", "清除", "削除", "지우기", "Loschen", "Effacer", "Cancella", "Clear", "Clear"],
        ["Tapped"] = ["点选", "點選", "タップ", "탭함", "Angetippt", "Touche", "Toccato", "Tapped", "Tapped"],
        ["Screen"] = ["画面", "畫面", "画面", "화면", "Bildschirm", "Ecran", "Schermata", "Screen", "Screen"],
        ["MobileInteractionAccessTitle"] = ["开启手机操作记录", "開啟手機操作紀錄", "スマホ操作記録を有効化", "휴대폰 조작 기록 켜기", "Smartphone-Aktionen erfassen", "Activer le suivi mobile", "Attiva registrazione mobile", "Enable phone activity records", "Enable mobile activity records"],
        ["MobileInteractionAccessMessage"] = ["若要记录手机上其他 App 的画面切换与点选项目，请在系统无障碍设定中开启 Memory Storage 服务。Android 需要使用者手动开启此权限。", "若要記錄手機上其他 App 的畫面切換與點選項目，請在系統無障礙設定中開啟 Memory Storage 服務。Android 需要使用者手動開啟此權限。", "他のアプリの画面切り替えやタップ項目を記録するには、システムのユーザー補助設定で Memory Storage サービスを有効にしてください。Android ではユーザーが手動で許可する必要があります。", "다른 앱의 화면 전환과 탭한 항목을 기록하려면 시스템 접근성 설정에서 Memory Storage 서비스를 켜세요. Android에서는 사용자가 직접 이 권한을 켜야 합니다.", "Um Bildschirmwechsel und angetippte Elemente anderer Apps zu erfassen, aktivieren Sie den Memory-Storage-Dienst in den Android-Bedienungshilfen. Android verlangt, dass der Benutzer diese Berechtigung manuell aktiviert.", "Pour enregistrer les changements d'ecran et les elements touches dans les autres apps, activez le service Memory Storage dans les reglages d'accessibilite Android. Android exige une activation manuelle.", "Per registrare schermate e tocchi nelle altre app, attiva il servizio Memory Storage nelle impostazioni di accessibilita di Android. Android richiede l'attivazione manuale.", "To record screen changes and tapped items inside other phone apps, enable the Memory Storage service in Android Accessibility settings. Android requires the user to turn this permission on manually.", "To record screen changes and tapped items inside other mobile apps, enable the Memory Storage service in Android Accessibility settings. Android requires the user to turn this permission on manually."],
    };

    public static readonly string[] LanguageNames =
    [
        "中文(简体)",
        "中文(繁體)",
        "日本語",
        "한국인",
        "Deutsch",
        "Français",
        "Italiano",
        "American(English)",
        "British(English)"
    ];

    static AppUi()
    {
        CurrentLanguage = AppLanguage.AmericanEnglish;
        IsDarkMode = Preferences.Default.Get(DarkModePreferenceKey, false);
    }

    public static event EventHandler? Changed;

    public static AppLanguage CurrentLanguage { get; private set; }

    public static bool IsDarkMode { get; private set; }

    public static string T(string key)
    {
        return Texts.TryGetValue(key, out var values) ? values[(int)CurrentLanguage] : key;
    }

    public static string DisplayAppName(string appName)
    {
        return appName == "Memory Storage" ? T("AppTitle") : appName;
    }

    public static string LocalizeRecordText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var lines = text.Replace("\r\n", "\n").Split('\n');
        return string.Join(Environment.NewLine, lines.Select(LocalizeRecordLine));
    }

    public static string RecordFileItemLabel => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "文件项目",
        AppLanguage.TraditionalChinese => "檔案項目",
        AppLanguage.Japanese => "ファイル項目",
        AppLanguage.Korean => "파일 항목",
        AppLanguage.German => "Dateielement",
        AppLanguage.French => "Element de fichier",
        AppLanguage.Italian => "Elemento file",
        _ => "File item"
    };

    private static string LocalizeRecordLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return line;
        }

        line = RemoveRecordFormattingMarks(line);

        var parts = Regex.Split(line, @"\s*(?:➡|→|\?\?)\s*")
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part.Trim())
            .ToList();

        if (parts.Count <= 1)
        {
            return LocalizeRecordPart(line);
        }

        return string.Join(" ➡ ", parts.Select(LocalizeRecordPart));
    }

    private static string LocalizeRecordPart(string part)
    {
        if (Regex.IsMatch(part, @"^\d{2}:\d{2}$"))
        {
            return part;
        }

        var colonIndex = part.IndexOf(':');
        if (colonIndex >= 0)
        {
            var label = part[..colonIndex].Trim();
            var value = part[(colonIndex + 1)..].Trim();
            return $"{LocalizeRecordLabel(label)} : {LocalizeRecordValue(value)}";
        }

        return LocalizeRecordValue(part);
    }

    private static string LocalizeRecordLabel(string label)
    {
        var normalized = NormalizeRecordText(label);

        if (normalized is "opened" or "open" or "開啟" or "打开" or "geoffnet" or "ouvert" or "aperto" or "열림" or "열린" or "開いた")
        {
            return T("Opened");
        }

        if (normalized is "tapped" or "tap" or "clicked" or "click" or "點選" or "点选" or "탭함" or "タップ")
        {
            return T("Tapped");
        }

        if (normalized is "screen" or "window" or "畫面" or "画面" or "화면" or "bildschirm" or "ecran" or "schermata")
        {
            return T("Screen");
        }

        if (normalized is "fileitem" or "檔案項目" or "文件项目" or "dateielement" or "elementdefichier" or "elementofile" or "파일항목" or "ファイル項目")
        {
            return RecordFileItemLabel;
        }

        if (normalized is "project" or "專案" or "专案" or "projet" or "projekt" or "progetto")
        {
            return T("Project");
        }

        if (normalized is "file" or "檔案" or "文件" or "fichier" or "datei")
        {
            return T("File");
        }

        return label;
    }

    private static string LocalizeRecordValue(string value)
    {
        var normalized = NormalizeRecordText(value);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return value;
        }

        if (normalized.All(character => character == '?'))
        {
            return LocalizedUnknownItem;
        }

        var uiText = LocalizeKnownUiText(normalized);
        if (!string.IsNullOrWhiteSpace(uiText))
        {
            return uiText;
        }

        var languageName = LocalizeLanguageName(normalized);
        if (!string.IsNullOrWhiteSpace(languageName))
        {
            return languageName;
        }

        return normalized switch
        {
            "memorystorage" or "記憶儲倉" or "记忆储仓" or "메모리저장소" => T("AppTitle"),
            "applaunch" or "應用程式啟動" or "应用程序启动" or "앱실행" or "アプリ起動" => LocalizedAppLaunch,
            "appoperation" or "應用程式操作" or "应用程序操作" or "앱작업" or "アプリ操作" => LocalizedAppOperation,
            "fileexplorer" or "檔案總管" or "档案总管" or "文件资源管理器" or "파일탐색기" or "ファイルエクスプローラー" => LocalizedFileExplorer,
            "thispc" or "本機" or "这台电脑" or "此电脑" or "내pc" or "このpc" => LocalizedThisPc,
            "quickaccess" or "常用" or "快速存取" or "快速访问" or "빠른실행" or "クイックアクセス" => LocalizedQuickAccess,
            "desktop" or "桌面" or "바탕화면" or "デスクトップ" or "bureau" => LocalizedDesktop,
            "pictures" or "圖片" or "图片" or "사진" or "ピクチャ" or "bilder" or "images" or "immagini" => LocalizedPictures,
            "downloads" or "下載" or "下载" or "다운로드" or "ダウンロード" or "telechargements" => LocalizedDownloads,
            "documents" or "文件" or "文档" or "문서" or "ドキュメント" or "dokumente" or "documenti" => LocalizedDocuments,
            "music" or "音樂" or "音乐" or "음악" or "ミュージック" or "musik" or "musique" or "musica" => LocalizedMusic,
            "videos" or "影片" or "视频" or "동영상" or "ビデオ" => LocalizedVideos,
            _ => value
        };
    }

    private static string? LocalizeKnownUiText(string normalized)
    {
        var key = KnownRecordUiKey(normalized);
        if (!string.IsNullOrWhiteSpace(key))
        {
            return T(key);
        }

        foreach (var item in Texts)
        {
            if (item.Value.Any(value => NormalizeRecordText(value) == normalized))
            {
                return T(item.Key);
            }
        }

        return null;
    }

    private static string? KnownRecordUiKey(string normalized)
    {
        return normalized switch
        {
            "recordopenedprograms" or "recordopenedprogrammes" or "記錄開啟的程式" or "记录打开的程序" or "열었던프로그램기록" or "開いたプログラムを記録" => "RecordOpenedPrograms",
            "recordactivityduration" or "記錄活動時長" or "记录活动时长" or "活動時間記録" or "활동시간기록" => "RecordActivityDuration",
            "timelineview" or "時間軸檢視" or "时间轴检视" or "タイムライン表示" or "타임라인보기" => "TimelineView",
            "searchfunctionrecords" or "搜尋功能記錄" or "搜索功能记录" or "検索機能記録" or "검색기능기록" => "SearchFunction",
            "appuserguide" or "app使用指南" or "应用程式使用指南" or "アプリ利用ガイド" or "앱사용자가이드" => "AppUserGuide",
            "privacystatement" or "隱私聲明" or "隐私声明" or "プライバシー声明" or "개인정보보호성명" => "PrivacyStatement",
            "chooselanguage" or "選擇語言" or "选择语言" or "言語を選択" or "언어선택" => "ChooseLanguage",
            "language" or "語言" or "语言" or "言語" or "언어" => "Language",
            "settings" or "設定" or "设置" or "설정" => "Settings",
            "start" or "開始" or "开始" or "시작" => "Start",
            "back" or "返回" or "뒤로" => "Back",
            "memoryrecordfile" or "記憶紀錄文件" or "记忆记录文件" or "メモリ記録ファイル" or "메모리기록파일" => "MemoryRecordFile",
            "login" or "登入" or "登录" or "로그인" => "Login",
            "register" or "註冊" or "注册" or "가입" => "Register",
            "profile" or "個人檔案" or "个人档案" or "프로필" => "Profile",
            "username" or "使用者姓名" or "用户名" or "사용자이름" => "UserName",
            "warning" or "警告" or "경고" => "Warning",
            "search" or "搜尋" or "搜索" or "검색" => "SearchLabel",
            "found" or "找到" or "찾음" => "FoundLabel",
            "opened" or "open" or "開啟" or "打开" or "열림" or "열린" or "開いた" => "Opened",
            _ => null
        };
    }

    private static string? LocalizeLanguageName(string normalized)
    {
        var language = normalized switch
        {
            "中文简体" or "simplifiedchinese" or "简体中文" => AppLanguage.SimplifiedChinese,
            "中文繁體" or "中文繁体" or "traditionalchinese" or "繁體中文" => AppLanguage.TraditionalChinese,
            "日本語" or "japanese" or "일본어" => AppLanguage.Japanese,
            "한국인" or "한국어" or "korean" => AppLanguage.Korean,
            "deutsch" or "german" => AppLanguage.German,
            "français" or "francais" or "french" => AppLanguage.French,
            "italiano" or "italian" => AppLanguage.Italian,
            "americanenglish" or "american" or "englishus" => AppLanguage.AmericanEnglish,
            "britishenglish" or "british" or "englishuk" => AppLanguage.BritishEnglish,
            _ => (AppLanguage?)null
        };

        return language is null ? null : LocalizedLanguageName(language.Value);
    }

    private static string LocalizedLanguageName(AppLanguage language)
    {
        return CurrentLanguage switch
        {
            AppLanguage.SimplifiedChinese => language switch
            {
                AppLanguage.SimplifiedChinese => "中文(简体)",
                AppLanguage.TraditionalChinese => "中文(繁体)",
                AppLanguage.Japanese => "日语",
                AppLanguage.Korean => "韩语",
                AppLanguage.German => "德语",
                AppLanguage.French => "法语",
                AppLanguage.Italian => "意大利语",
                AppLanguage.AmericanEnglish => "美式英语",
                AppLanguage.BritishEnglish => "英式英语",
                _ => LanguageNames[(int)language]
            },
            AppLanguage.TraditionalChinese => language switch
            {
                AppLanguage.SimplifiedChinese => "中文(簡體)",
                AppLanguage.TraditionalChinese => "中文(繁體)",
                AppLanguage.Japanese => "日文",
                AppLanguage.Korean => "韓文",
                AppLanguage.German => "德文",
                AppLanguage.French => "法文",
                AppLanguage.Italian => "義大利文",
                AppLanguage.AmericanEnglish => "美式英文",
                AppLanguage.BritishEnglish => "英式英文",
                _ => LanguageNames[(int)language]
            },
            AppLanguage.Japanese => language switch
            {
                AppLanguage.SimplifiedChinese => "中国語(簡体)",
                AppLanguage.TraditionalChinese => "中国語(繁体)",
                AppLanguage.Japanese => "日本語",
                AppLanguage.Korean => "韓国語",
                AppLanguage.German => "ドイツ語",
                AppLanguage.French => "フランス語",
                AppLanguage.Italian => "イタリア語",
                AppLanguage.AmericanEnglish => "アメリカ英語",
                AppLanguage.BritishEnglish => "イギリス英語",
                _ => LanguageNames[(int)language]
            },
            AppLanguage.Korean => language switch
            {
                AppLanguage.SimplifiedChinese => "중국어(간체)",
                AppLanguage.TraditionalChinese => "중국어(번체)",
                AppLanguage.Japanese => "일본어",
                AppLanguage.Korean => "한국어",
                AppLanguage.German => "독일어",
                AppLanguage.French => "프랑스어",
                AppLanguage.Italian => "이탈리아어",
                AppLanguage.AmericanEnglish => "미국 영어",
                AppLanguage.BritishEnglish => "영국 영어",
                _ => LanguageNames[(int)language]
            },
            AppLanguage.German => language switch
            {
                AppLanguage.SimplifiedChinese => "Chinesisch (vereinfacht)",
                AppLanguage.TraditionalChinese => "Chinesisch (traditionell)",
                AppLanguage.Japanese => "Japanisch",
                AppLanguage.Korean => "Koreanisch",
                AppLanguage.German => "Deutsch",
                AppLanguage.French => "Franzosisch",
                AppLanguage.Italian => "Italienisch",
                AppLanguage.AmericanEnglish => "Amerikanisches Englisch",
                AppLanguage.BritishEnglish => "Britisches Englisch",
                _ => LanguageNames[(int)language]
            },
            AppLanguage.French => language switch
            {
                AppLanguage.SimplifiedChinese => "Chinois simplifie",
                AppLanguage.TraditionalChinese => "Chinois traditionnel",
                AppLanguage.Japanese => "Japonais",
                AppLanguage.Korean => "Coreen",
                AppLanguage.German => "Allemand",
                AppLanguage.French => "Français",
                AppLanguage.Italian => "Italien",
                AppLanguage.AmericanEnglish => "Anglais americain",
                AppLanguage.BritishEnglish => "Anglais britannique",
                _ => LanguageNames[(int)language]
            },
            AppLanguage.Italian => language switch
            {
                AppLanguage.SimplifiedChinese => "Cinese semplificato",
                AppLanguage.TraditionalChinese => "Cinese tradizionale",
                AppLanguage.Japanese => "Giapponese",
                AppLanguage.Korean => "Coreano",
                AppLanguage.German => "Tedesco",
                AppLanguage.French => "Francese",
                AppLanguage.Italian => "Italiano",
                AppLanguage.AmericanEnglish => "Inglese americano",
                AppLanguage.BritishEnglish => "Inglese britannico",
                _ => LanguageNames[(int)language]
            },
            _ => language switch
            {
                AppLanguage.SimplifiedChinese => "Simplified Chinese",
                AppLanguage.TraditionalChinese => "Traditional Chinese",
                AppLanguage.Japanese => "Japanese",
                AppLanguage.Korean => "Korean",
                AppLanguage.German => "German",
                AppLanguage.French => "French",
                AppLanguage.Italian => "Italian",
                AppLanguage.AmericanEnglish => "American English",
                AppLanguage.BritishEnglish => "British English",
                _ => LanguageNames[(int)language]
            }
        };
    }

    private static string NormalizeRecordText(string text)
    {
        return RemoveRecordFormattingMarks(text).Trim()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .ToLowerInvariant();
    }

    private static string RemoveRecordFormattingMarks(string text)
    {
        return text
            .Replace("\uFE0E", string.Empty)
            .Replace("\uFE0F", string.Empty)
            .Replace("\u200B", string.Empty)
            .Replace("\u200C", string.Empty)
            .Replace("\u200D", string.Empty);
    }

    private static string LocalizedFileExplorer => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "文件资源管理器",
        AppLanguage.TraditionalChinese => "檔案總管",
        AppLanguage.Japanese => "ファイル エクスプローラー",
        AppLanguage.Korean => "파일 탐색기",
        AppLanguage.German => "Datei-Explorer",
        AppLanguage.French => "Explorateur de fichiers",
        AppLanguage.Italian => "Esplora file",
        _ => "File Explorer"
    };

    private static string LocalizedAppLaunch => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "应用程序启动",
        AppLanguage.TraditionalChinese => "應用程式啟動",
        AppLanguage.Japanese => "アプリ起動",
        AppLanguage.Korean => "앱 실행",
        AppLanguage.German => "App-Start",
        AppLanguage.French => "Lancement de l'app",
        AppLanguage.Italian => "Avvio app",
        _ => "App launch"
    };

    private static string LocalizedAppOperation => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "应用程序操作",
        AppLanguage.TraditionalChinese => "應用程式操作",
        AppLanguage.Japanese => "アプリ操作",
        AppLanguage.Korean => "앱 작업",
        AppLanguage.German => "App-Aktion",
        AppLanguage.French => "Action de l'app",
        AppLanguage.Italian => "Operazione app",
        _ => "App operation"
    };

    private static string LocalizedThisPc => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "此电脑",
        AppLanguage.TraditionalChinese => "本機",
        AppLanguage.Japanese => "この PC",
        AppLanguage.Korean => "내 PC",
        AppLanguage.German => "Dieser PC",
        AppLanguage.French => "Ce PC",
        AppLanguage.Italian => "Questo PC",
        _ => "This PC"
    };

    private static string LocalizedQuickAccess => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "快速访问",
        AppLanguage.TraditionalChinese => "快速存取",
        AppLanguage.Japanese => "クイック アクセス",
        AppLanguage.Korean => "빠른 실행",
        AppLanguage.German => "Schnellzugriff",
        AppLanguage.French => "Acces rapide",
        AppLanguage.Italian => "Accesso rapido",
        _ => "Quick access"
    };

    private static string LocalizedDesktop => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "桌面",
        AppLanguage.TraditionalChinese => "桌面",
        AppLanguage.Japanese => "デスクトップ",
        AppLanguage.Korean => "바탕 화면",
        AppLanguage.German => "Desktop",
        AppLanguage.French => "Bureau",
        AppLanguage.Italian => "Desktop",
        _ => "Desktop"
    };

    private static string LocalizedPictures => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "图片",
        AppLanguage.TraditionalChinese => "圖片",
        AppLanguage.Japanese => "ピクチャ",
        AppLanguage.Korean => "사진",
        AppLanguage.German => "Bilder",
        AppLanguage.French => "Images",
        AppLanguage.Italian => "Immagini",
        _ => "Pictures"
    };

    private static string LocalizedDownloads => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "下载",
        AppLanguage.TraditionalChinese => "下載",
        AppLanguage.Japanese => "ダウンロード",
        AppLanguage.Korean => "다운로드",
        AppLanguage.German => "Downloads",
        AppLanguage.French => "Telechargements",
        AppLanguage.Italian => "Download",
        _ => "Downloads"
    };

    private static string LocalizedDocuments => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "文档",
        AppLanguage.TraditionalChinese => "文件",
        AppLanguage.Japanese => "ドキュメント",
        AppLanguage.Korean => "문서",
        AppLanguage.German => "Dokumente",
        AppLanguage.French => "Documents",
        AppLanguage.Italian => "Documenti",
        _ => "Documents"
    };

    private static string LocalizedMusic => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "音乐",
        AppLanguage.TraditionalChinese => "音樂",
        AppLanguage.Japanese => "ミュージック",
        AppLanguage.Korean => "음악",
        AppLanguage.German => "Musik",
        AppLanguage.French => "Musique",
        AppLanguage.Italian => "Musica",
        _ => "Music"
    };

    private static string LocalizedVideos => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "视频",
        AppLanguage.TraditionalChinese => "影片",
        AppLanguage.Japanese => "ビデオ",
        AppLanguage.Korean => "동영상",
        AppLanguage.German => "Videos",
        AppLanguage.French => "Videos",
        AppLanguage.Italian => "Video",
        _ => "Videos"
    };

    private static string LocalizedUnknownItem => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => "未知项目",
        AppLanguage.TraditionalChinese => "未知項目",
        AppLanguage.Japanese => "不明な項目",
        AppLanguage.Korean => "알 수 없는 항목",
        AppLanguage.German => "Unbekanntes Element",
        AppLanguage.French => "Element inconnu",
        AppLanguage.Italian => "Elemento sconosciuto",
        _ => "Unknown item"
    };

    public static CultureInfo Culture => CurrentLanguage switch
    {
        AppLanguage.SimplifiedChinese => new CultureInfo("zh-CN"),
        AppLanguage.TraditionalChinese => new CultureInfo("zh-TW"),
        AppLanguage.Japanese => new CultureInfo("ja-JP"),
        AppLanguage.Korean => new CultureInfo("ko-KR"),
        AppLanguage.German => new CultureInfo("de-DE"),
        AppLanguage.French => new CultureInfo("fr-FR"),
        AppLanguage.Italian => new CultureInfo("it-IT"),
        AppLanguage.BritishEnglish => new CultureInfo("en-GB"),
        _ => new CultureInfo("en-US")
    };

    public static void SetLanguage(AppLanguage language)
    {
        CurrentLanguage = language;
        Changed?.Invoke(null, EventArgs.Empty);
    }

    public static void SetDarkMode(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        Preferences.Default.Set(DarkModePreferenceKey, isDarkMode);

        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;
        }

        Changed?.Invoke(null, EventArgs.Empty);
    }

    public static Color PageBackground => IsDarkMode ? Color.FromArgb("#101418") : Color.FromArgb("#F4F7FA");
    public static Color Surface => IsDarkMode ? Color.FromArgb("#171D23") : Colors.White;
    public static Color SoftSurface => IsDarkMode ? Color.FromArgb("#223029") : Color.FromArgb("#EAF5EF");
    public static Color Text => IsDarkMode ? Color.FromArgb("#EEF3F6") : Color.FromArgb("#14171A");
    public static Color MutedText => IsDarkMode ? Color.FromArgb("#AAB6C2") : Color.FromArgb("#5F6B7A");
    public static Color Border => IsDarkMode ? Color.FromArgb("#2D3843") : Color.FromArgb("#E1E6EC");
    public static Color Primary => Color.FromArgb("#1E8E5A");
    public static Color CabinetInk => IsDarkMode ? Color.FromArgb("#E8EDF2") : Color.FromArgb("#333740");
}
