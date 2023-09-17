﻿#define DEBUG_LEVEL_1
// #define DEBUG_LEVEL_2
// #define DEBUG_LEVEL_3

using System;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;
using Microsoft.CodeAnalysis;

public class MyBot : IChessBot
{
    // Encoded weights
    readonly decimal[] weights = {
        930887103832024506651837443m,930882399912283515987886852m,933304936880393403720991746m,933309677838017010111546116m,
        1242789965148765124905075459m,622611038276754466828124675m,1242794706394898336172934147m,76141821567766778023569328653m,
        78925813073770267474311447051m,1256107039062521609446819087m,5580430212835145486346424538m,2131411760333761017103250689m,
        75826291577846936898591786254m,78611572585633068576437175039m,1849694541563140868857794319m,3378957184559579619603519978m,
        1195646580985435528842314754m,78612946260250896109834339609m,2805916882738692832444613375m,1233118503256673624199006726m,
        8357313672982964841107228647m,2750334778176392684814795011m,1236778116051999105710097942m,1559514362643674430120722943m,
        315548676254007696168193034m,7748010263384572688494429931m,279342274338324559625585155m,1862982378237041920136384010m,
        1885919630236623390973427977m,78918715505459178363966653431m,1548643585319439578968493805m,2450563490662850270892391938m,
        2787844017121035065288166151m,1862968929850733231180546558m,78910295581435045976101947649m,4952964415645257297922623476m,
        2434809933969559943469990147m,930919588905262821585191680m,5605793747991590176223459334m,75493861955589064307438060555m,
        1862959391222497166099614685m,2118095018320089685887878402m,932105105743863669123059721m,4659218980246611630006073852m,
        75537349840714432361623586546m,1559486046528133891342144234m,2729763742797691721785475332m,621406797858359540313162754m,
        1242785261228743754514629124m,622606297319413434892354306m,622611001311492399337571331m,622611038204978352045097730m,
        933304937024790071051682308m,622611037560981235365110774m,1577803829415420510575986416m,2767221700599108015909769973m,
        8932786104941869775447520008m,624840381125915006160991964m,1823069525732728839161643726m,608132115650097149637295358m,
        74271571559679868856184866571m,1253708334785879431856717821m,77991652319353226818029880611m,305843972885779914189371909m,
        1555925363901916079738390257m,2201491880695734431211781064m,78923564730490408119090607618m,3400703720760062841360089592m,
        2194200362600285659975909110m,76771649064705241833146550288m,78917477801988934220229186041m,2369536830968955962030030847m,
        76753566974503241942853416447m,4337625915358581436452373787m,591244729322003243704911892m,7396208034700898176210173914m,
        8336818602524698508674596094m,77373646529116770320817588478m,929659288833909322754096381m,78274434098673290203767186708m,
        5580401601785346826140977328m,2408199103968215302999571453m,931027778699239750984208405m,2786588163080880152529140239m,
        77066688244488203395865378547m,78576566031656835630355451406m,77674711560224217178464582400m,78612904753714956602467617044m,
        288961605158858610829884410m,78640714494819768196669707225m,3416429143648590102779532538m,78867940767751931590143376642m,
        78922261929757626029181835258m,5263658296742242166293854424m,76124981921701264315077431295m,74907433485437389448677622798m,
        3002976864917818179030288125m,2384757956029938002890240m,3700545692958819814655458289m,2483147819547781916526909141m,
        78291306210495191917256841979m,619022151366386299181204737m,78622386824314666452363325456m,6821982493396710800102391539m,
        10551556723346436335990734339m,1202853335937256577462565882m,8346518287359606449313212943m,71187592037142856221566821357m,
        76457252165329959147451055885m,1268158130088928072409941471m,3391003572094674164923626502m,2740663258135509544789606143m,
        78923498689840548353881869797m,75158894593754317978524646156m,77034023285040872508785820685m,7981328242432017326560971462m,
        5507940873827396872280930811m,60947608960291303064852m,1549800436125490264874876695m,5921295347364347800958213625m,
        5263611127839824950021584539m,3010244346617816286316463617m,78918837310093084492873137172m,945394103201978046996480024m,
        77354364941331407488125962991m,4643465404678163737237195975m,74581093501307695555006496257m,1250153338717243883914495m,
        1536530641865604487666465545m,930934512181316614969559829m,1560708659620748270181157301m,2770900425383202820847238656m,
        5260131351773743849491337226m,5226200669905940015937028598m,75794828000307024494272188185m,1877433297498098089473605089m,
        4532295032186062301318022653m,77371242292118172647136953594m,3117815022118188083641123856m,78965825280371164922958052357m,
        78313015297505314448262955260m,7995840371300617658064565755m,1236683483812139647655614216m,1849656578228858650037651463m,
        79183441575552593592543937050m,14195183490995295986391379168m,78565671441888439317520646658m,75512027958145690393703813656m,
        1252437758792732439256625655m,77970951882240081956463316008m,77987800122972001874176970515m,10743827356770893912032021503m,
        78921071874027210098733287936m,3399457087491751709198650121m,77340939845238077572617600496m,2173638958009502572864604900m,
        70528831393769741809769516038m,1246335300287134179826669553m,2481905912884949077726405168m,77032701206319453864779576580m,
        3701707376740874358242478537m,567052470609778977659746813m,78597121776411546258261803518m,936908009982643325159013374m,
        78315447464979029235861358092m,1247602185647716124925367032m,891025701371976588671778809m,78913841026820423401975838706m,
        3426072068721160113485640962m,75189250390722002860907496184m,76696634474383124960120995789m,75723482173521281607099415553m,
        74890606217919667346748934932m,322693449420087394008434204m,75196328960202305741425486066m,7104790965426563388157396960m,
        76094626919891952285380114940m,77062956928311802814848305922m,4673754367929590667742935000m,76141794043867879870517354247m,
        4344850563513104357069293250m,28110037276784980769328921342m,78604384018949402365055076654m,79218467551645901564666644734m,
        310703602455152734162980866m,2162716344535892439733631199m,78875156103382274281511121404m,4632853272892271506827380784m,
        344543655746642334074929660m,76441602704841840214411906284m,936870064744949647558243053m,10804165161934912364599182837m,
        78915130175516876866099676161m,79155697782137459425565341438m,78012069194877087950141722071m,3089957688404387505418936790m,
        7691209953212028247175132929m,78925822041402576960599761909m,4679709346141096988220650528m,76429560763628769341349696011m,
        5909186923055422659637414066m,6390518831040554444773260027m,78917453637186560812819023342m,620160019319341446480000480m,
        72984173994928701631922965971m,5285428055013117019435498940m,9844315821723419115422219524m,79227004814897645669079121918m,
        79201575627714036165452560403m,76766831992282227502645513740m,1862950186517324409377458399m,3966519040143048053271427072m,
        78303347654944666401585237247m,626185830862759796238841104m,76178047725993047683122331175m,77735172883867253720665357559m,
        1403603532535060198386691572m,283195504155510702113547020m,1565502081586264510331946256m,3463421745561672608299482901m,
        925938283553350857931614935m,70238518749587777500184637176m,70862357695007777543717321453m,78922209630141406931258831137m,
        73086824073561006701514064428m,9924005645381419087062558922m,568233836150716460865819595m,71478910951889852003762370022m,
        77369906393825187558263221805m,73983790657891793475858075158m,5621429207558010407734276097m,1762680586406913513963386356m,
        70859919994168362886069156090m,78299584572555915254280091420m,72971924505694089737151445791m,1010609982248051610012093936m,
        77911652017212340147509061879m,73031104777894384731786117846m,1216056536580109868637287662m,74011632919449005065940435996m,
        2803405024990004840492293596m,76060810183074990800567331569m,74571417887363884061043523849m,1835059483012238805102749725m,
        70257971451413245688581924657m,1546117616382924879264082135m,78232079673797470087186411764m,72718067994494369195184951037m,
        600746314356672783040441123m,72432908450292327393564297276m,1903941027705582184455138307m,4625346035231031524691077856m,
        75195166960996859243476155363m,3686019435972498732704332522m,74588347258285831567396700988m,974290520443418497353644233m,
        77927477610608865750587469043m,74260742620977746743724741662m,4013567458756834679929368031m,70289350543367717899881611789m,
        75232654174412811707826899197m,1832708502199707586602723057m,72101530345458559466130898672m,11788150036409594068200844063m,
        70892666231869440376179395357m,1605260073120938619719190535m,3109386359309647597180875768m,67779531906408069177342949162m,
        1241618892625587592507033889m,1570422808796611006451417107m,77713416555236777385527870978m,3710184042957666421294887170m,
        78925950336797079803847441903m,78285082446224241673227728392m,4335202730446623413460268318m,13047067641367010943848089102m,
        1886018965232599817397797121m,78612871514917868423946437627m,77984126028832664899560999696m,1554650048464869612686734350m,
        4693012013580950477701441282m,71209433941165143465443328007m,76437036674656314387589687331m,78628558790873869673528954128m,
        2796216661872411708848998659m,77385797454858276299142670601m,78025286434437717374745242117m,77367634087147017929723603717m,
        2767217367121794368467045135m,2185694918747791241717155333m,4963896953249318322153856769m,5581672342334165949239657479m,
        626327841978019999646679042m,13680290074772492130308323338m,3113097322955193576458828814m,76157472246402863019088748044m,
        1568179848653613666749846790m,76906416145239493062030073850m,6870325286025276740594304793m,5920108926926308325264263936m,
        1258473534322239298886828337m,76161302011259011297479232516m,1549819733227713059796157687m,9936265333683616462692154876m,
        78014457659912316239396599324m,354206098781357343198667050m,4076814686497094696647852837m,9903850972824144048005840922m,
        5576822086185510204217491229m,77401513641693491824083271180m,77438975599990379825550400031m,14283392708281787429635099655m,
        11750830613610837677769359657m,15182847368971179516277684758m,323997432463141138650235654m,78582606088199307522822439961m,
        649206908031419927786031106m,4654336588404128507098894839m,76730602218403744114093000140m,1268309819305308343614958113m,
        930887122279047860592968451m,622611038205262021750096899m,932100751873724242907628035m,623824667872275282646008578m,
        622611001239433710116471555m,932100770392807385631687683m,930887122486208955227964419m,1241590594235364839401326605m,
        4655587535545890941026240256m,2772104831527870459456781062m,2795111444501408360821756463m,1230710207157830731059887875m,
        313149751690275824652714251m,75828691480528902096460840952m,79216097219620202679257206525m,2743076296898593930887823106m,
        1241594468700420242806799873m,620188815333958157256163846m,639540721973998587769652480m,4022105663885508404425590492m,
        909145033609218565236396035m,5573237772611645099670047234m,3404457926671343251326044446m,334881933659989906576703989m,
        2163991550812995713813120506m,1526896867457544761135921935m,5267369689625875227893958912m,78924703594635636985512461836m,
        1270609389062014033591861500m,1843621374841957417074885101m,4028159573133621525685204730m,929668254232397265003350274m,
        621378648200091930864845066m,76448846814375793263656698611m,1532932217196793108623262190m,3406771775633333720243633665m,
        75531319119370679595682562560m,2172439459265931636653622526m,2508554187461592073843639287m,2456547078551691747909634024m,
        2131368981890362643344265215m,1233085206792427959840671489m,1865353576412470261587509758m,623853020662032891862319590m,
        909126549689901300289111014m,2730977299106415124584203014m,4029396630169271961580799486m,933309696212141639358087684m,
        622611038132639278735426307m,932091288694015529419277316m,932091307213098676421657092m,622615742196775841547158531m,
        932096029579860817758913028m,1241585798009552458164798464m,78894498859360342783942067483m,79223274902856861944533216507m,
        7422804755000877630860622298m,5274510296362766781046073376m,3460030772770655607371204564m,76477856074331363797896661505m,
        4405495489312090133305560832m,10225061196778181618902302983m,627385258265060604691738909m,78633389993434461216477809130m,
        79193146630355847180725852924m,9572368848519723325765594387m,77038963712210051114796580355m,1245132312817709255347989776m,
        77356721826118384619893819109m,78566904183463479268748950993m,76094683200752172005889548002m,77981782407457071352089347846m,
        630979884422840421325538821m,73972976254467131568112338684m,2445680655750628049418059972m,3381389074527102721916734232m,
        933294975991284920379705095m,930877290813536809006268687m,1258539094550841743541535955m,1548676808698147661019017680m,
        3383792796473491502020553231m,663734308996776681428880129m,3096162823635673000718105104m,78315433039336928936422868466m,
        1506321589985786544867310551m,78315465469360399597197985011m,3738139751472725752197351175m,3720964782796508182622698976m,
        1292370165215845083898054059m,4012434423786989242403131115m,79212460295573077379808359197m,77974515053885071947248961032m,
        76758582349479643827635345957m,78933208300622807592152202739m,78259860229138023437853988056m,74830137422198514483042714338m,
        77406353177997125724165113599m,627399150394656846721776894m,4868797459340097240105957m,3092517838902372718655514110m,
        73968244351092138015285704973m,3138380152015374083121153284m,65908142318550056709227411693m,65684500352842540534589817877m,
        2503770688032277977735754985m,77966086148019179486242536391m,3352280444633805205665416186m,78592238760144961327075888673m,
        13038407060390110072070211544m,4906987329043648796471321822m,1526909890280130651397223909m,583948691242264391072941313m,
        1242742999090961993640440565m,79212554759703683641019333101m,4639815310094028520722203048m,73957377632971722387315825960m,
        3699364750630404608335481606m,1861802227700845619841794569m,77385792696101858272940655865m,2468659652636613059612771562m,
        1223446451483101868832653540m,3380090368468550937619531527m,2791437738248668260592320272m,927284030872497680112749050m,
        888551180550196222943558113m,1506287869452752994366387718m,77413615163791810653328245002m,4030657815185314409716973333m,
        77995086624003165166039141108m,77961232015587990026267265230m,2120498297673261624810540059m,77645687765678020514929182727m,
        939312009620431146948298756m,77731540831426334244379033833m,2163968030850352398565637166m,3364468484853718079391791598m,
        77378449102546413312808387336m,2484290449058900769353503249m,78953769391549142400693570040m,2398471121613159298460675811m,
        78871566623653543419370929432m,75185532485471497918911156232m,621402168171732446434230800m,78316637279828224803968453088m,
        918774251894136627437633456m,78880015084221636328169208816m,75251995587803105502502584580m,3715039319450071410220991753m,
        2801104828042842916034119894m,3405449973550607381086275044m,565810487715034104676744453m,3694405381451523888456926226m,
        78923517836071657623651747092m,4035451259200113576497186856m,12432692279090767387825806259m,77366439737050245956653940732m,
        74260747100884231023935815947m,314519273404067992212663548m,78617607881103328934608373717m,3099690063192514447073416445m,
        284111218120954689985380609m,73673383846031510974026742792m,319180231199170552075386902m,77072671464017773210432897773m,
        3051338359497656346587233517m,1184728285417042904783519236m,946588122317947572345767941m,77381041791780569846469489418m,
        76468189756618806231147283186m,1497887608666987043205087651m,7997091152118540599669688071m,295010385841129796920214788m,
        78309368929615508870648500525m,3414011421716699041603194552m,3071810391644242300924986077m,79179857003806503421023354854m,
        77070294711430509893436898056m,1549880623467549393450430731m,76446429165652704744149221611m,1541470459860704565312481757m,
        6159538111894044031018990832m,72105529708797575348080808205m,2486831135406353468611756287m,48390201475245073016818936m,
        914028477144380423800159957m,9532450314052674280922021858m,10850047382417467113567157772m,75838437836414159282859002931m,
        76460959886814989709531941086m,274332452539796302269251320m,77322913777058950168667619056m,77370033567401447891889879567m,
        3423771980378071385648855553m,76436861595962031057367269091m,4937282617163034860225243590m,5856097839816718917243958793m,
        1218615581760796278438296069m,944137714270989623044866583m,78004767548649634670323630309m,3919370581344827254523108828m,
        2427573314605470215755594227m,78935559229623643705002884354m,1248810060212896583328722216m,76770459046369370033356540124m,
        610503351495612251022228169m,3356058079278006714599737581m,11137917877362118506801991432m,1244042020257382181886098443m,
        337347175130567168397282028m,3042828840720956219137458640m,1829151177786737286116214269m,932090992963182124188961285m,
        3109366781769674622673879298m,77073894632166371985635810509m,5871685874619431374531196922m,77707521790159025966341755186m,
        16189834558026864774060131588m,8064777454677365451420400904m,17680757839870178807885795589m,11771338836360668210277250020m,
        74882211050855682177754264587m,4284370771797228805182002705m,7450610582829728564237699067m,3103411989901077311425486330m,
        7064882283605925097239149839m,74201561150267708964897161499m,65996243445537812621080598819m,8688583511722199068884075268m,
        77034085285055203848338805734m,9567424860991957809210396895m,76143097197802230089797210672m,5625075262070752219675431448m,
        6528186199684265645917799941m,69915897076674108174757145369m,5534277898292510673989009128m,76438046579850313335313922856m,
        53249606128670450509222931m,9313570158731647850937781240m,77403964621379213534632807618m,4312295048947111414080797695m,
        73571673296380073308366960939m,76452401722726956977943418899m,7139921111591873151395107342m,79225853371812216731905892299m,
        6182422936719423309043723236m,74864062350119176988259650839m,74282370597808115291333265683m,5292579434072889480258520826m,
        935836496130607741897806573m,8970276601206977234297945847m,72732712682973606939565748007m,2236474450878837669838986773m,
        9302746920050017061701682429m,616647021598594390432160997m,6756464066933053188687263406m,74822959389187266736355672316m,
        5061687090318681285461153044m,7139944723114967193315835399m,78316684762397398534508580334m,1528092271161298813977427726m,
        75365720155775720851198242110m,74610330334049915228830775318m,5892277255175477404079549670m,326551957618118001094760645m,
        9846681818561926556564454079m,1216301362411484113089859590m,18309095815042089952080426787m,4645939852155991812255052513m,
        2789034312457022953538912773m,76752486125505215155464370180m,74925623929740728060139342024m,78639491586586289983993155068m,
        4337682413919236226701724181m,71510408157933149275519706892m,3421231680480588565773221140m,77989065790918683717168857608m,
        7745535982868674860124801793m,4338825527736699074895150827m,4907402932380518365309897209m,932322780127436115334854159m,
        78323995040478877187475630088m,71210505364009779569549836550m,3099756913696211138951778822m,280485197275300108083855880m,
        2774475422331518901954684162m,20118986231784598016375713294m,68109714587857136255485608202m,8352563034126897233570170666m,
        1902864514004231031578102807m,3109342984747558209815305718m,1870284078881723474052837653m,78945623643706704049698508825m,
        6193313302827131302330825998m,3070700267735997647322746124m,4364349750261073709836336381m,642086355079140048034800154m,
        7477220530249194132127812910m,2506269116862601567853210863m,1234313428656760611890661389m,3112988099627542296793192973m,
        7453070400736599266725265421m,73398849937269053273161988638m,1866605779384404298071732720m,77030363875290937720218261018m,
        2184486879233201206959862293m,4969946030464364904954334223m,3758513077185936367323179525m,10494581759362131244499538706m,
        1223504872764377569249136157m,5905626534930010003650972418m,77711003500053025552685204509m,2510901498753722145691862023m,
        628656132083254399069325051m,76168272133531637764815782735m,2479520506423687890240210702m,1539019293193950668647891182m,
        73364962548622620191857181705m,76448885311866265523700762873m,585106077602022244002564887m,2496464763824004863750772235m,
        643200848710260821573765925m,9219434874697168202458523107m,
    };
    readonly double scaling_factor = 28.244;
    readonly double shift = -0.10824465751647949;

    #if DEBUG_LEVEL_1
        public MyBot()
        {
            ChessChallenge.Chess.Board testBoard = new ();
            testBoard.LoadPosition("r1b1r1k1/5q1p/p1p2p1b/3pn2N/6p1/4B1N1/PPP2PPP/R2QR1K1 b - - 11 24");
            Console.WriteLine("MyBot loaded!");
            Console.WriteLine(
                "Eval of Test FEN (r1b1r1k1/5q1p/p1p2p1b/3pn2N/6p1/4B1N1/PPP2PPP/R2QR1K1 b - - 11 24): " +
                Evaluate(new Board(testBoard)));
        }
    #endif

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        double[] evaluations = new double[moves.Length];

        // evaluate each move
        for (int i = 0; i < moves.Length; i++)
        {
            #if DEBUG_LEVEL_1
                Console.WriteLine("\n-----------------------\n" + moves[i].ToString());
            #endif

            // make move
            board.MakeMove(moves[i]);

            // evaluate board
            evaluations[i] = Evaluate(board);

            #if DEBUG_LEVEL_1
                Console.WriteLine("\nEval: " + evaluations[i]);
            #endif

            // undo move
            board.UndoMove(moves[i]);
        }

        // find best move
        return moves[evaluations.ToList().IndexOf(evaluations.Max())];
    }

    private unsafe double Evaluate(Board board)
    {
        // load board state
        ulong[] boardState = board.GetAllPieceLists().Select(pieceList =>
            board.GetPieceBitboard(
                pieceList.TypeOfPieceInList,
                pieceList.IsWhitePieceList ^ board.IsWhiteToMove // invert board if bot plays black (white to move)
            )
        ).ToArray();

        // create network
        int[] layers = {768, 1}; // optimize later
        double[] neurons = new double[1]; // optimize later

        #if DEBUG_LEVEL_1
            int networkConnections = 0;
            for(int i = 1; i < layers.Length; i++)
            {
                networkConnections += layers[i-1] * layers[i];
            }
            Debug.Assert(weights.Length * 12 >= networkConnections, "Number of weights (" + weights.Length * 12 + ") needs to be larger than the number of network connections (" + networkConnections +")!");
        #endif

        #if DEBUG_LEVEL_2
            Console.WriteLine("\nBoard state:\n");
            Console.WriteLine("FEN: " + board.GetFenString() + "\n");
            Console.WriteLine("Binary: ");
            for (int s = 0; s < 768; s++)
            {
                Console.Write(boardState[s / 64] >> (s % 64) & 1);
                if ((s + 1) % 8 == 0) Console.Write(" ");
                if ((s + 1) % 64 == 0) Console.WriteLine();
            }
        #endif

        // compute feed forward
        #if DEBUG_LEVEL_3
            Console.WriteLine("\nNeurons:\n");
        #endif

        fixed (double * neuron_po = neurons)
        {
            fixed(decimal * weight_po = weights)
            {
                double * input_p = neuron_po;
                double * output_p = neuron_po;
                sbyte * weight_p = (sbyte *) weight_po;

                // loop through layers
                for (int l = 1; l < layers.Length; l++)
                {
                    // loop through neurons
                    for(int n = 0; n < layers[l]; n++)
                    {
                        // loop through inputs
                        for(int i = 0; i < layers[l - 1]; i++)
                        {
                            // skip 4 bytes at the beginning of each group of 16 weights (one decimal)
                            if ((weight_p - (sbyte*) weight_po) % 16 == 0) weight_p += 4;

                            // compute weighted sum
                            *output_p +=
                                (l == 1 ? (boardState[i / 64] >> (i % 64) & 1) : *input_p++) * // for the first layer, use the board state as input
                                (((double) *weight_p++) / scaling_factor + shift); // decode sbyte [-128, 127] to double [-2.0, 2.0]

                            #if DEBUG_LEVEL_3
                                input_p--; weight_p--; // undo increment temporarily
                                double input = l == 1 ? (boardState[i / 64] >> (i % 64) & 1) : *input_p;
                                Console.WriteLine(
                                    Math.Round(input, 2).ToString().PadRight(8) + "("+ (l == 1 ? i : input_p - neuron_po) +") * " +
                                    Math.Round(((double) *weight_p) / scaling_factor + shift, 2).ToString().PadRight(8) + "(" + (weight_p - (sbyte*) weight_po) + ") -> " +
                                    Math.Round(*output_p, 2).ToString().PadRight(8) + "(" + (output_p - neuron_po) + ")"
                                );
                                input_p++; weight_p++; // redo increment
                            #endif
                        }

                        // apply bias and ReLU activation function
                        if(l < layers.Length - 1) {
                            *output_p += *weight_p++;
                            *output_p *= Convert.ToDouble(*output_p > 0);
                        }

                        // increment/reset pointers
                        output_p++;
                        input_p -= l > 1 ? layers[l - 1] : 0;
                    }
                }
            }
        }

        // return value of output neuron
        return neurons[^1];
    }
}
