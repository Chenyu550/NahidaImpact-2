using NahidaImpact.Common.Data;
using NahidaImpact.Common.Data.Binout;
using NahidaImpact.Common.Data.Excel;
using NahidaImpact.Common.Data.Provider;
using NahidaImpact.Gameserver;
using NahidaImpact.Gameserver.Controllers.Dispatching;
using NahidaImpact.Gameserver.Game;
using NahidaImpact.Gameserver.Game.Entity;
using NahidaImpact.Gameserver.Game.Entity.Factory;
using NahidaImpact.Gameserver.Game.Entity.Listener;
using NahidaImpact.Gameserver.Game.Scene;
using NahidaImpact.Gameserver.Network;
using NahidaImpact.Gameserver.Network.Kcp;
using NahidaImpact.Gameserver.Network.Session;
using NahidaImpact.Gameserver.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.Title = "NahidaImpact | Game Server [Experimental] (4.3.50)";

Console.WriteLine("                                                ddxocc:;lc:o            \r\n                                            lllooddocc;',ll:::llcdxxo:,l\r\n                                   dllllooxO0KXXXKK00OOkkkko::;ccclc:olc\r\n                                 llodkO0KK00K00KXXXKK0KKXNNNKx::ldOkxx:;ld\r\n                              l:coxxO0XXK00KK0kOKXXKOO0000KXXxodddxkkdcdocld\r\n                            oclxOOOKXK0KKXNXK0KNXXXXXKKXNXKKOxdooxxoddcdKOl:o\r\n                          dc:d000KXK0KXNNNXKKNNNNKOKNNX0KNNNOxdc:lxxdko:o00d:l\r\n                         dccxOOXNXKKXNNNNK0KNNNNNK0KXNNX0KN0kkdc,:oloOx:ldx0xccd\r\n                       xdco000NNXXNWWNWNKk0NNNNNNXXKXNNNXK0kxocc:::ccodlclox0Olcd\r\n                      xd:l0KKNNKXWMMMMWKxONWNNNNNNNKKNNNNKkxoclc::::cccll:;lkOOlcd\r\n                      d::k0KNX0XWMWWWN0xxKMMWWWWWWWXXWWNNNXK0kxddd::ldxooxc,coxOlcd\r\n                     xl:dkOKX0KWWNNNN0OxxXWWWWWWWMMNXWWXKXK0XNX0kOd:cxK0oodl:;:xOccd\r\n                     dclxk0X0KNNNNNNK0KddXNNXNNNWWWNKXNXO00dOXN0oodxocodddc;clccxkcc\r\n                     o;oO0X00NNNNNXKKX0dxKNNXXNNNNNX00N0x0XkdOKkko,::',cdkl'',,;ckx:l\r\n                     c;o0KKKNNNNNX0KKOxkxkXNKKXNNNNN0OKklxKkdoodOx;'..':xOxl;'..,oOd:o\r\n                 xolc;'l0KKXNNNXK0KKxkKNKOXN0OKNNNNN0xxdox0Kkkkllddc;'';dKKkdol:;:x0dcd\r\n                dl:cl;;x0XNNNXXKKKOx0NMWXkOXKxOXNNNNKxod00O000XKdclddlcclONXkooooookkll\r\n                ::dkxox0XXKK0OO0OdccddddddxkKOd0NNNNXkoO0Oxolldoddxkoddood0XNOoccloldlcd\r\n               ;ck0xdOKKOO0KX0dolc'...',,,;oxxll0NNNXkkOc'';,...'okxokK0Oxx0NXxcccox0kcl\r\n               :oxkdkNN0k0XXNXxc'.;:''dK0c'oK0xodOKXN0kd',xKO:.;:,'lx0NNX0odXN0l:ldxKKll\r\n               :cc:oKN0coKXXNKo..:x:,cd0k:,xWWWNKOkO00Ox;,d0xl,,xl..c0NNXKl:ONXolxdkKKol\r\n                ::;dXXl'dX0KNNKdcxKdo0OOkooKWWMWMWWNXXXKdokOO0do0OllONNKK0o;dXXxkOxOX0ll\r\n                xd:oX0xdkKKOOXNX00XKO0K000KWMWWWWWWWWWWWX00KK0OOXX0KNKOkK0xxx00xOOkKXd:o\r\n                 d::xk0kx0KKkxkOOOO0K0KKXNWWWWWWWWWWWWWWWNXKKKK0OkOkxdxKXkk0xdddxk0XO\r\n                     ccc:dOXX0dllx0XNXXNNNWWWWWWWWWWWWWWWNNNXXXXKklldOXNOlloldkOO0Kk:\r\n                      xxl;cxKNKOocd0NNNNNWWWWWWNXXXNWWWWWWNNNNNKxclOKNXOookxdkKKK0d;\r\n                       xd:;coOXNKx;;ok0XNWWWWWWN000XWWWWWWNX0ko;,oKNXKx:,cxxddO0xl:\r\n                         o:clok0XN0l;,;lodxkO0XXXXXXXKOkxool:';cOXX0xllc;ldddddc,\r\n                          oc:looxkOOd:;;,...':codxdoc:,...,,,:dOkxdlldl:dOkl;          Game Server\r\n                           xolccllll:,';oolll;',:::,';cllod:',:llldxxd:ckdl");
Console.CursorTop = 0;
Console.CursorVisible = false;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Logging.ClearProviders();

builder.Services.Configure<GatewayOptions>(builder.Configuration.GetSection(GatewayOptions.Section));

// Resources
builder.Services.UseLocalAssets();
builder.Services.AddSingleton<DataHelper>();
builder.Services.AddSingleton<ExcelTableCollection>();
builder.Services.AddSingleton<BinDataCollection>();

// Game Logic
builder.Services.AddScoped<Player>();
builder.Services.AddScoped<SceneManager>();
builder.Services.AddScoped<EntityManager>();
builder.Services.AddScoped<EntityFactory>();

// Logic Listeners
builder.Services.AddScoped<IEntityEventListener, SessionEntityEventListener>();

// Network
builder.Services.AddScoped<NetCommandDispatcher>();
builder.Services.AddScoped<NetSession, KcpSession>();
builder.Services.AddSingleton<IGateway, KcpGateway>();
builder.Services.AddSingleton<NetSessionManager>();

builder.Services.AddHostedService<GameServer>();

await builder.Build().RunAsync();