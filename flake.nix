{
  description = "qua2ma Dev Environment";
  inputs.nixpkgs.url = "github:nixos/nixpkgs/nixos-25.11";

  outputs = { self, nixpkgs }:
  let
    pkgs = import nixpkgs { 
      system = "x86_64-linux";
      config.allowUnfree = true;
    };
    
    libs = with pkgs; [
      stdenv.cc.cc.lib
      zlib
      icu
      openssl
    ];
  in
  {
    devShells.x86_64-linux.default = pkgs.mkShell {
      nativeBuildInputs = [ pkgs.dotnet-sdk_10 ];

      shellHook = ''
        export NIX_LD_LIBRARY_PATH=${pkgs.lib.makeLibraryPath libs}:$NIX_LD_LIBRARY_PATH
        export DOTNET_ROLL_FORWARD="LatestMajor"
        export NIX_LD_LIBRARY_PATH=$NIX_LD_LIBRARY_PATH:$PWD/Quaver.Shared
      '';
    };
  };
}