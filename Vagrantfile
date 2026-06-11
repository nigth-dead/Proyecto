Vagrant.configure("2") do |config|
  config.vm.box = "ubuntu/jammy64"

  config.vm.hostname = "servidor-docker-punto-venta"

  config.vm.network "forwarded_port", guest: 8080, host: 8080
  config.vm.network "forwarded_port", guest: 3306, host: 3307

  config.vm.provider "virtualbox" do |vb|
    vb.name = "Servidor Docker Punto Venta"
    vb.memory = "2048"
    vb.cpus = 2
  end

  config.vm.provision "shell", inline: <<-SHELL
    apt-get update
    apt-get install -y puppet curl ca-certificates gnupg
  SHELL

  config.vm.provision "puppet" do |puppet|
    puppet.manifests_path = "manifests"
    puppet.manifest_file = "site.pp"
  end
end